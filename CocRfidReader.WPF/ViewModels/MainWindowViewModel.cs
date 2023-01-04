﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using CocRfidReader.Services;
using CocRfidReader.WPF.Services;
using CommunityToolkit.Mvvm.Input;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CocRfidReader.WPF.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private CocReader cocReader;
        private IConfiguration configuration;
        private ILogger<MainWindowViewModel>? logger;
        private IMessagingService? messagingService;
        private PacklisteReader packlisteReader;
        private IAccountsService accountsService;
        private MediaPlayer klaxonSound = new();
        private ISendGridClient sendGridClient;

        private ImpinjReader? reader;
        private ConcurrentObservableDictionary<string, Tag> tags = new();
        private CocsViewModel cocsViewModel;
        private ObservableCollection<AccountViewModel> accounts = new();
        private bool loadingStarted;
        private AccountViewModel? selectedAccount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AccountViewModel? SelectedAccount
        {
            get => selectedAccount;
            set
            {
                selectedAccount = value;
                RaisePropertyChanged(nameof(SelectedAccount));
                StartReadCommand.NotifyCanExecuteChanged();
            }
        }
        public bool LoadingStarted
        {
            get => loadingStarted;
            set
            {
                loadingStarted = value;
                RaisePropertyChanged(nameof(LoadingStarted));
                StartReadCommand.NotifyCanExecuteChanged();
                FinishLoadingCommand.NotifyCanExecuteChanged();
            }
        }

        public CocsViewModel CocsViewModel
        {
            get => cocsViewModel;
            set
            {
                cocsViewModel = value;
                RaisePropertyChanged(nameof(CocsViewModel));
            }
        }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get => accounts;
            set
            {
                accounts = value;
                RaisePropertyChanged(nameof(Accounts));
            }
        }

        public IAsyncRelayCommand StartReadCommand { get; private set; }
        public IAsyncRelayCommand FinishLoadingCommand { get; private set; }

        public MainWindowViewModel
            (CocReader cocReader,
            PacklisteReader packlisteReader,
            ReaderService readerService,
            IConfiguration configuration,
            IAccountsService accountsService,
            ISendGridClient sendGridClient,
            CocsViewModel cocsViewModel,
            ILogger<MainWindowViewModel>? logger = null,
            IMessagingService? messagingService = null)
        {
            this.cocReader = cocReader;
            this.configuration = configuration;
            tags.CollectionChanged += Tags_CollectionChanged;
            this.logger = logger;
            this.messagingService = messagingService;
            this.packlisteReader = packlisteReader;

            ConfigureReader(readerService);

            StartReadCommand = new AsyncRelayCommand(StartRead, () => CanRead());
            FinishLoadingCommand = new AsyncRelayCommand(FinishLoading, () => LoadingStarted);
            this.cocsViewModel = cocsViewModel;
            this.accountsService = accountsService;
            GetAccounts();
            this.sendGridClient = sendGridClient;
        }

        private async Task FinishLoading()
        {
            reader?.Stop();
            LoadingStarted = false;
            StartReadCommand.NotifyCanExecuteChanged();

            var notifyEmails = configuration.GetSection("notifyAddresses").Get<string[]>();
            var ccEmails = configuration.GetSection("ccAddresses").Get<string[]>();

            var file = await SaveCocsToTemp();

            if (notifyEmails == null || !notifyEmails.Any())
            {
                messagingService?.DisplayMessage("Nie skonfigurowano żadnego adresu email.\n\rCOC zostaną zapisane w pliku tekstowym w głównym folderze programu.", MessageType.Info);
                SaveCocsLocally(file);
            }
            else
            {
                var response = await SendCocsByEmail(notifyEmails, ccEmails, file);
                await ProcessSendGridResponse(response, file);
            }
            ClearTemp();
            InitializeCollections();
        }

        private void ClearTemp()
        {
            var directory = new DirectoryInfo(@".\Temp");
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }
        }

        private async Task ProcessSendGridResponse(Response? response, string file)
        {
            if (response != null && !response.IsSuccessStatusCode)
            {
                var responseMessage = await response.Body.ReadAsStringAsync();
                messagingService?.DisplayMessage($"{responseMessage}\r\n\r\nPlik został zapisany w głównym folderze programu", MessageType.Error);
                logger?.LogWarning(responseMessage);
                SaveCocsLocally(file);
            }
            else messagingService?.DisplayMessage("Wiadomość wysłana.", MessageType.Info);
        }

        private async Task<string> SaveCocsToTemp()
        {
            try
            {
                var directory = new DirectoryInfo(@".\Temp");
                if (!directory.Exists) directory.Create();

                var fileName = $@".\Temp\COC_{DateTime.Now:yyyy-MM-dd_hhmmss}.txt";
                using (var file = new FileInfo(fileName).CreateText())
                {
                    foreach (var coc in CocsViewModel.Cocs)
                    {
                        await file.WriteLineAsync($"L;{DateTime.Now:yyyyMMddhhmmss};{coc.PRODUKTIONSNR}");
                    }
                    await file.FlushAsync();
                }
                return fileName;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
        }

        private void SaveCocsLocally(string file)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                fileInfo.CopyTo(fileInfo.Name);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
            
        }

        private async Task<Response?> SendCocsByEmail(string[] notifyEmails, string[]? ccEmails, string file)
        {
            if (SelectedAccount == null) return new Response(System.Net.HttpStatusCode.ExpectationFailed, new StringContent("Nie zaznaczono konta klienta."), null);
            try
            {
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(configuration.GetValue<string>("fromAddress"), "Coc skaner na rampa1"),
                    Subject = "Nowy załadunek"
                };

                await AddMessageContent(file, msg);

                PopulateToField(notifyEmails, msg);

                if (ccEmails != null && ccEmails.Any())
                {
                    PopulateCCField(ccEmails, msg);
                }

                return await sendGridClient.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
        }

        private async Task AddMessageContent(string file, SendGridMessage msg)
        {
            msg.AddContent(MimeType.Text, $@"Na rampie 1 skończono załadunek do:
Nazwa: {SelectedAccount.AccountName};
Numer konta: {SelectedAccount.AccountNumber};
Adres: {SelectedAccount.ZipCity}

Numery COC znajdują się w załączniku do tej wiadomości.");

            using (var fileStream = File.OpenRead(file))
            {
                await msg.AddAttachmentAsync("COC.txt", fileStream);
            }
        }

        private static void PopulateToField(string[] notifyEmails, SendGridMessage msg)
        {
            foreach (var email in notifyEmails)
            {
                msg.AddTo(new EmailAddress(email));
            }
        }

        private static void PopulateCCField(string[]? ccEmails, SendGridMessage msg)
        {
            foreach (var cc in ccEmails)
            {
                msg.AddCc(new EmailAddress(cc));
            }
        }

        private void InitializeCollections()
        {
            tags.Clear();
            CocsViewModel.Clear();
        }

        private async void GetAccounts()
        {
            accounts = new ObservableCollection<AccountViewModel>(await accountsService.GetAccounts());
        }

        private void ConfigureReader(ReaderService readerService)
        {
            try
            {
                reader = readerService.Reader;
                reader.TagsReported += Reader_TagsReported;
                reader.ConnectionLost += Reader_ConnectionLost;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
            }
        }

        private bool CanRead()
        {
            return reader != null && reader.IsConnected && !LoadingStarted && selectedAccount != null;
        }

        private async Task StartRead()
        {
            LoadingStarted = true;

            try
            {
                reader?.Stop();
                reader?.Start();
                logger?.LogInformation("Read started");

            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
            }
        }

        private async void Tags_CollectionChanged(object? sender, DictionaryChangedEventArgs<string, Tag> e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                try
                {
                    var epcValue = e.NewValue.Epc.ToHexString();
                    logger?.LogInformation("Getting COC from the database");
                    var coc = await cocReader.GetAsync(epcValue);

                    if (coc == null)
                    {
                        logger?.LogWarning($"Could not get coc number from EPC: {epcValue}");
                        return;
                    }

                    logger?.LogInformation($"Database returned: {coc}");

                    var cocVM = new CocViewModel(coc);
                    if (GetHammingDistance(coc.AccountNumber, SelectedAccount.AccountNumber) < 2)
                    {
                        cocVM.IsAccountCorrect = true;
                    }
                    else
                    {
                        await App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                        {
                            klaxonSound.Open(new Uri(@".\Sounds\vuvuzela-37503.mp3", UriKind.Relative));
                            klaxonSound.Play();
                        });
                    }

                    CocsViewModel.AddCoc(cocVM);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex.Message);
                    messagingService?.DisplayMessage(ex.Message, MessageType.Error);
                }
            }
        }

        private int GetHammingDistance(string s, string t)
        {
            if (s.Length != t.Length)
            {
                return 99;
            }

            int distance =
                s.ToCharArray()
                .Zip(t.ToCharArray(), (c1, c2) => new { c1, c2 })
                .Count(m => m.c1 != m.c2);

            return distance;
        }

        private void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Reader_TagsReported(ImpinjReader reader, TagReport report)
        {
            foreach (Tag tag in report)
            {
                var epcValue = tag.Epc.ToHexString();
                if (epcValue.StartsWith("888"))
                    tags.TryAdd(tag.Epc.ToString(), tag);
            }
        }

        private void Reader_ConnectionLost(ImpinjReader reader)
        {
            StartReadCommand.NotifyCanExecuteChanged();
        }

    }
}
