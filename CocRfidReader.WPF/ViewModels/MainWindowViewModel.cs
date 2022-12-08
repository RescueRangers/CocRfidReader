using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using CocRfidReader.Model;
using CocRfidReader.Services;
using CocRfidReader.WPF.Services;
using CommunityToolkit.Mvvm.Input;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private Timer timer = new();

        private ImpinjReader? reader;
        private ConcurrentObservableDictionary<string, Tag> tags = new();
        private ObservableCollection<CocViewModel> cocs = new();
        private string packingList;
        private ObservableCollection<CocViewModel> expectedCocs = new();
        private int timerLength = 1;
        private int timerValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CocViewModel> Cocs
        {
            get => cocs;
            set
            {
                cocs = value;
                RaisePropertyChanged(nameof(Cocs));
            }
        }

        public ObservableCollection<CocViewModel> ExpectedCocs
        {
            get => expectedCocs;
            set
            {
                expectedCocs = value;
                RaisePropertyChanged(nameof(ExpectedCocs));
            }
        }

        public string PackingList
        {
            get => packingList;
            set
            {
                packingList = value;
                RaisePropertyChanged(nameof(PackingList));
            }
        }

        public int TimerLength
        {
            get => timerLength;
            set
            {
                timerLength = value;
                RaisePropertyChanged(nameof(TimerLength));
            }
        }

        public int TimerValue
        {
            get => timerValue;
            set
            {
                timerValue = value;
                RaisePropertyChanged(nameof(TimerValue));
            }
        }

        public IAsyncRelayCommand StartReadCommand { get; private set; }

        public MainWindowViewModel
            (CocReader cocReader,
            PacklisteReader packlisteReader,
            ReaderService readerService,
            IConfiguration configuration,
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
            ConfiureTimer();

            StartReadCommand = new AsyncRelayCommand(StartRead, () => CanRead());
        }

        private void ConfigureReader(ReaderService readerService)
        {
            try
            {
                reader = readerService.Reader;
                reader.TagsReported += Reader_TagsReported;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
            }
        }

        private void ConfiureTimer()
        {
            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
        }

        private bool CanRead()
        {
            return true;
            //return reader != null && reader.IsConnected;
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

                    if (ExpectedCocs.Contains(cocVM)) cocVM.BackgroundBrush = Brushes.Green;
                    else cocVM.BackgroundBrush = Brushes.Red;

                    await App.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Cocs.Add(cocVM);
                    });
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

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            TimerValue -= 1;
            if (TimerValue <= 0)
            {
                timer.Stop();
                reader?.Stop();
            }
        }


        private async Task StartRead()
        {
            if (string.IsNullOrWhiteSpace(PackingList))
            {
                return;
            }

            try
            {
                await GetExpectedCocs();
                StartReadTimer();

                reader?.Stop();
                Cocs.Clear();
                reader?.Start();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService?.DisplayMessage(ex.Message, MessageType.Error);
            }

        }

        private void StartReadTimer()
        {
            TimerLength = configuration.GetValue<int>("readerReadTime") / 1000;
            TimerValue = TimerLength;
            timer.Start();
        }

        private async Task GetExpectedCocs()
        {
            var expectedCocs = await packlisteReader.GetCocs(PackingList);
            var cocs = expectedCocs.Select(c => new CocViewModel(c));
            ExpectedCocs = new ObservableCollection<CocViewModel>(cocs);
        }
    }
}
