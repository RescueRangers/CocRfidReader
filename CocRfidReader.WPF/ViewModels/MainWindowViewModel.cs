using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.Model;
using CocRfidReader.Services;
using CocRfidReader.WPF.Services;
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
        private ServiceProvider? serviceProvider;
        private IMessagingService? messagingService;

        private ImpinjReader reader;
        private ConcurrentObservableDictionary<string, Tag> tags = new();
        private ObservableCollection<Coc?> cocs;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Coc?> Cocs
        {
            get => cocs;
            set
            {
                cocs = value;
                RaisePropertyChanged(nameof(Cocs));
            }
        }

        public MainWindowViewModel(CocReader cocReader, ReaderService readerService, IConfiguration configuration, ILogger<MainWindowViewModel>? logger = null, ServiceProvider serviceProvider = null, IMessagingService? messagingService = null)
        {
            Cocs = new();

            Cocs.Add(new Coc
            {
                PRODUKTIONSNR = 59437,
                ItemNumber = "A9B1067899",
                Name = "Dupa",
                ItemText = "Mamma Mia!"
            });

            this.cocReader = cocReader;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            tags.CollectionChanged += Tags_CollectionChanged;
            this.logger = logger;
            this.messagingService = messagingService;

            try
            {
                reader = readerService.Reader;
                reader.TagsReported += Reader_TagsReported;
                StartRead();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                messagingService.DisplayMessage(ex.Message, MessageType.Error);
            }
        }

        private async void Tags_CollectionChanged(object? sender, DictionaryChangedEventArgs<string, Tag> e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                try
                {
                    var epcValue = e.NewValue.Epc.ToHexString();
                    logger?.LogInformation($"Tag added with epc: {e.NewValue.Epc}");
                    if (epcValue.StartsWith("888"))
                    {
                        logger?.LogInformation("Getting COC from the database");
                        var coc = await cocReader.GetAsync(epcValue);

                        if (coc == null) return;

                        logger?.LogInformation($"Database returned: {coc}");

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Cocs.Add(coc);
                        });
                    }
                    //else if (epcValue.StartsWith("777"))
                    //{
                    //    logger?.LogInformation("Getting Item from the database");
                    //    var item = await itemReader.GetAsync(epcValue);

                    //    if (item == null) return;

                    //    logger?.LogInformation($"Database returned {item.ItemNumber}");
                    //    items.TryAdd(epcValue, item);
                    //}
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex.Message);
                    serviceProvider?.GetService<IMessagingService>().DisplayMessage(ex.Message, MessageType.Error);
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

        private void StartRead()
        {
            Cocs.Clear();
            reader.Start();
        }
    }
}
