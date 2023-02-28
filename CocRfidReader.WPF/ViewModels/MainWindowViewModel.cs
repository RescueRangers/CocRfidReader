using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CocRfidReader.Model;
using CocRfidReader.Services;
using CocRfidReader.WPF.Messages;
using CocRfidReader.WPF.Services;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
        private TagsService tagsService;
        private ILogger<MainWindowViewModel>? logger;
        private IMessagingService? messagingService;
        private AccountsJsonService accountsService;
        private MediaPlayer klaxonSound = new();
        private ISendGridClient sendGridClient;
        private ReaderService readerService;
        private ConfigurationService configurationService;

        private ImpinjReader? reader;
        private ConcurrentObservableDictionary<string, Tag> tags = new();
        private CocsViewModel cocsViewModel;
        private ObservableCollection<AccountViewModel> accounts = new();
        private bool loadingStarted;
        private AccountViewModel? selectedAccount;
        private bool connecting;
        private bool connected;
        private CancellationTokenSource cancellationTokenSource;

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
                CancelLoadingCommand.NotifyCanExecuteChanged();
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

        public bool Connecting
        {
            get => connecting;
            set
            {
                connecting = value;
                RaisePropertyChanged(nameof(Connecting));
            }
        }

        public bool Connected
        {
            get => connected; 
            set
            {
                connected = value;
                RaisePropertyChanged(nameof(Connected));
                RaisePropertyChanged(nameof(ConnectionButtonText));
                App.Current.Dispatcher.Invoke(() => StartReadCommand.NotifyCanExecuteChanged());
            }
        }

        public string ConnectionButtonText
        {
            get => Connected ? "Odłącz" : "Połącz";
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

        public IRelayCommand StartReadCommand { get; private set; }
        public IAsyncRelayCommand FinishLoadingCommand { get; private set; }
        public IRelayCommand ConnectionToggleCommand { get; set; }
        public IRelayCommand OpenSettingsCommand { get; set; }
        public IRelayCommand OpenAccountsCommand { get; set; }
        public IRelayCommand CancelLoadingCommand { get; set; }

        public MainWindowViewModel
            (
                ReaderService readerService,
                ConfigurationService configurationService,
                AccountsJsonService accountsService,
                TagsService tagsService,
                ISendGridClient sendGridClient,
                CocsViewModel cocsViewModel,
                ILogger<MainWindowViewModel>? logger = null,
                IMessagingService? messagingService = null
            )
        {
            tags.CollectionChanged += Tags_CollectionChanged;
            this.logger = logger;
            this.messagingService = messagingService;
            this.readerService = readerService;
            this.cocsViewModel = cocsViewModel;
            this.accountsService = accountsService;
            this.sendGridClient = sendGridClient;

            GetAccounts();
            accountsService.AccountsChanged += AccountsService_AccountsChanged;
            SetUpCommands();
            this.configurationService = configurationService;
            this.tagsService = tagsService;

#if DEBUG
            PopulateCocs();
#endif
        }

        private void PopulateCocs()
        {
            for (int i = 0; i < 5; i++)
            {
                var coc = new CocViewModel(new Model.Coc
                {
                    AccountNumber = $"12354-{i}",
                    PRODUKTIONSNR = 2744+i,
                    Name = $"item-{i}",
                    ItemText = "Test item"
                });
                cocsViewModel.AddCoc(coc);
            }
        }

        private void AccountsService_AccountsChanged(object? sender, EventArgs e)
        {
            GetAccounts();
        }

        private void SetUpCommands()
        {
            StartReadCommand = new RelayCommand(StartRead, () => CanRead());
            ConnectionToggleCommand = new RelayCommand(() => ToggleConnection(readerService));
            FinishLoadingCommand = new AsyncRelayCommand(FinishLoading, () => LoadingStarted);
            OpenSettingsCommand = new RelayCommand(() => WeakReferenceMessenger.Default.Send<OpenSettingsMessage>());
            OpenAccountsCommand = new RelayCommand(() => WeakReferenceMessenger.Default.Send<OpenAccountsMessage>());
            CancelLoadingCommand = new RelayCommand(CancelLoading, () => LoadingStarted);
        }

        private void CancelLoading()
        {
            cancellationTokenSource.Cancel();
            LoadingStarted = false;
            try
            {
                reader?.Stop();
                InitializeCollections();
                logger?.LogInformation("Read cancelled");

            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
            }
        }

        private void StartRead()
        {
            LoadingStarted = true;
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                reader?.Stop();
                InitializeCollections();
                reader?.Start();
                logger?.LogInformation("Read started");

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

        private void ToggleConnection(ReaderService readerService)
        {
            Connecting = true;
            Task task = Task.Run(DoConnect);
            
        }

        private void DoConnect()
        {
            try
            {
                if (!Connected)
                {
                    reader = readerService.Connect();
                    reader.TagsReported += Reader_TagsReported;
                    reader.ConnectionLost += Reader_ConnectionLost;
                    Connecting = false;
                    Connected = true;
                }
                else
                {
                    reader.Disconnect();
                    reader.TagsReported -= Reader_TagsReported;
                    reader.ConnectionLost -= Reader_ConnectionLost;
                    Connecting = false;
                    Connected = false;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
                Connecting = false;
                Connected = false;
                return;
            }
            
        }

        private async Task FinishLoading()
        {
            reader?.Stop();
            LoadingStarted = false;
            StartReadCommand.NotifyCanExecuteChanged();

            if (CocsViewModel.Cocs.Any()) await SaveCocsAndNotify();

            InitializeCollections();
        }

        private async Task SaveCocsAndNotify()
        {
            var notifyEmails = configurationService.GetSettings().NotifyAddresses;

            var file = await SaveCocsToTemp();

            if (notifyEmails == null || !notifyEmails.Any())
            {
                messagingService?.DisplayMessage("Nie skonfigurowano żadnego adresu email.\n\rCOC zostaną zapisane w pliku tekstowym w głównym folderze programu.", MessageType.Info);
                SaveCocsLocally(file);
            }
            else
            {
                var response = await SendCocsByEmail(notifyEmails, file);
                await ProcessSendGridResponse(response, file);
            }
            ClearTemp();
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

        private async Task<Response?> SendCocsByEmail(List<string> notifyEmails, string file)
        {
            if (SelectedAccount == null) return new Response(System.Net.HttpStatusCode.ExpectationFailed, new StringContent("Nie zaznaczono konta klienta."), null);
            try
            {
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(configurationService.GetSettings().FromAddress, "Coc skaner na rampa1"),
                    Subject = "Nowy załadunek"
                };

                await AddMessageContent(file, msg);

                PopulateToField(notifyEmails, msg);

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
            msg.AddContent(MimeType.Text, $@"Na rampie {configurationService.GetSettings().RampNumber.GetValueOrDefault(1)} skończono załadunek do:
Nazwa: {SelectedAccount.AccountName}; {SelectedAccount.ZipCity}
Numer konta: {SelectedAccount.AccountNumber};

Numery COC znajdują się w załączniku do tej wiadomości.");

            using var fileStream = File.OpenRead(file);
            await msg.AddAttachmentAsync("COC.txt", fileStream);
            fileStream.Close();
        }

        private static void PopulateToField(List<string> notifyEmails, SendGridMessage msg)
        {
            foreach (var email in notifyEmails)
            {
                msg.AddTo(new EmailAddress(email));
            }
        }

        private void InitializeCollections()
        {
            tags.Clear();
            CocsViewModel.Clear();
        }

        private async void GetAccounts()
        {
            Accounts = new ObservableCollection<AccountViewModel>(await accountsService.GetAccounts());
        }

        private async void Tags_CollectionChanged(object? sender, DictionaryChangedEventArgs<string, Tag> e)
        {
            if(cancellationTokenSource.IsCancellationRequested) return;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                try
                {
                    var coc = await tagsService.GetCocFromTag(e.NewValue, SelectedAccount.AccountNumber);
                    if (!coc.IsAccountCorrect)
                    {
                        await App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
                        {
                            klaxonSound.Open(new Uri(@".\Sounds\vuvuzela-37503.mp3", UriKind.Relative));
                            klaxonSound.Play();
                        });
                    }
                    CocsViewModel.AddCoc(coc);
                }
                catch (SqlException ex)
                {
                    logger?.LogError(ex.Message);
                    messagingService?.DisplayMessage(ex.Message, MessageType.Error);
                }
            }
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
            App.Current.Dispatcher.Invoke(() =>
            {
                StartReadCommand.NotifyCanExecuteChanged();
            });
            reader.Disconnect();
            Connected = false;
        }
    }
}
