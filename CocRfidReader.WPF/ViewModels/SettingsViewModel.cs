using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.Models;
using CocRfidReader.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.WPF.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private ConfigurationService configurationService;
        private ILogger<SettingsViewModel> logger;
        private string? readerIp;
        private int? enabledAntennas;
        private double? antena1Power;
        private double? antena2Power;
        private double? antena3Power;
        private double? antena4Power;
        private ObservableCollection<string>? notifyAddresses;
        private SettingsModel settings;

        public IRelayCommand SaveSettingsCommand { get; set; }
        public IRelayCommand AddAddressCommand { get; set; }

        public SettingsViewModel(ConfigurationService configurationService, ILogger<SettingsViewModel> logger)
        {
            this.configurationService = configurationService;
            this.logger = logger;

            GetSettings();
            ConfigureCommands();
        }

        private void ConfigureCommands()
        {
            AddAddressCommand = new RelayCommand(() => NotifyAddresses.Add(""));
            SaveSettingsCommand = new RelayCommand(() => configurationService.SaveSettings(settings));
        }

        private void GetSettings()
        {
            settings = configurationService.GetSettings();
            ReaderIp = settings.ReaderIP;
            EnabledAntennas = settings.EnabledAntennas;
            Antena1Power = settings.Antena1Power;
            Antena2Power = settings.Antena2Power;
            Antena3Power = settings.Antena3Power;
            Antena4Power = settings.Antena4Power;

            if (settings.NotifyAddresses == null)
                NotifyAddresses = new ObservableCollection<string>();
            else
                NotifyAddresses = new ObservableCollection<string>(settings.NotifyAddresses);
        }

        public string? ReaderIp 
        { 
            get => readerIp; 
            set 
            {
                readerIp = value;
                settings.ReaderIP = value;
                OnPropertyChanged();
            } 
        }
        public int? EnabledAntennas 
        { 
            get => enabledAntennas; 
            set 
            {
                enabledAntennas = value;
                settings.EnabledAntennas = value;
                OnPropertyChanged();
            } 
        }
        public double? Antena1Power 
        { 
            get => antena1Power; 
            set 
            {
                antena1Power = value;
                settings.Antena1Power = value;
                OnPropertyChanged();
            } 
        }
        public double? Antena2Power 
        { 
            get => antena2Power; 
            set 
            {
                antena2Power = value;
                settings.Antena2Power = value;
                OnPropertyChanged();
            } 
        }
        public double? Antena3Power 
        { 
            get => antena3Power; 
            set 
            {
                antena3Power = value;
                settings.Antena3Power = value;
                OnPropertyChanged();
            } 
        }
        public double? Antena4Power
        {
            get => antena4Power; set
            {
                antena4Power = value;
                settings.Antena4Power = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string>? NotifyAddresses
        {
            get => notifyAddresses; 
            set
            {
                notifyAddresses = value;
                settings.NotifyAddresses = value?.ToList();
                OnPropertyChanged();
            }
        }
    }
}
