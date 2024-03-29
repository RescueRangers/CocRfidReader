﻿using System.Collections.ObjectModel;
using System.Windows.Threading;
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
            DeleteCocCommand = new RelayCommand(DeleteCoc);
        }

        public void AddCoc(CocViewModel coc)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                var cocCount = Cocs.Count;
                coc.Number = cocCount + 1;
                Cocs.Add(coc);
            });
        }

        private void DeleteCoc()
        {
            Cocs.RemoveAt(SelectedCocIndex);
        }

        internal void Clear()
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                Cocs.Clear();
            });
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
