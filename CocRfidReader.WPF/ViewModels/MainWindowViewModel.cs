﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CocRfidReader.Services;
using CocRfidReader.WPF.Services;
using CommunityToolkit.Mvvm.Input;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        private ImpinjReader? reader;
        private ConcurrentObservableDictionary<string, Tag> tags = new();
        private string? packingList;
        private int timerLength = 1;
        private int timerValue;
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
        }

        private Task FinishLoading()
        {
            throw new NotImplementedException();
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

        private void Reader_ConnectionLost(ImpinjReader reader)
        {
            StartReadCommand.NotifyCanExecuteChanged();
        }

        private bool CanRead()
        {
            return reader != null && reader.IsConnected && !LoadingStarted && selectedAccount != null;
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

                    if (coc == null) return;

                    logger?.LogInformation($"Database returned: {coc}");
                    var cocVM = new CocViewModel(coc);
                    cocVM.BackgroundBrush.Freeze();

                }
                catch (Exception ex)
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

        private async Task StartRead()
        {
            LoadingStarted = true;
            

            try
            {
                reader?.Stop();
                reader?.Start();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
            }
        }
    }
}
