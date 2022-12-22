using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CocRfidReader.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.WPF.ViewModels
{
    public class CocsViewModel : ObservableObject
    {
        public CocsViewModel(ILogger<CocsViewModel>? logger)
        {
            this.logger = logger;

#if DEBUG
            for (int i = 0; i < 118; i++)
            {
                var coc = new CocViewModel(new Coc
                {
                    PRODUKTIONSNR = 597631,
                    ItemNumber = "A9B10331961",
                    AccountNumber = "96313334" + $"_{i}",
                    Name = "Precut LPS glass kit suction side CB"
                });

                if (i%5 == 0)
                {
                    coc.BackgroundBrush = Brushes.Red;
                }

                Cocs.Add(coc);
            }
#endif
            DeleteCocCommand = new RelayCommand(DeleteCoc);
        }

        private void DeleteCoc()
        {
            Cocs.RemoveAt(SelectedCocIndex);
        }

        private ILogger<CocsViewModel>? logger;
        private ObservableCollection<CocViewModel> cocs = new();
        private int selectedCocIndex;

        public int SelectedCocIndex
        {
            get => selectedCocIndex;
            set
            {
                selectedCocIndex = value;
                OnPropertyChanged();
            }
        }
        public IRelayCommand DeleteCocCommand { get; set; }
        public ObservableCollection<CocViewModel> Cocs
        {
            get => cocs;
            set
            {
                cocs = value;
                OnPropertyChanged();
            }
        }
    }
}
