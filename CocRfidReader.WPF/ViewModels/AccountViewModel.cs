using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CocRfidReader.WPF.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CocRfidReader.WPF.ViewModels
{
    public class AccountViewModel : ObservableObject, IEquatable<AccountViewModel>
    {
        private string zipCity;
        private string accountName;
        private string accountNumber;

        public string AccountNumber
        {
            get => accountNumber;
            set
            {
                accountNumber = value;
                OnPropertyChanged();
            }
        }
        public string AccountName
        {
            get => accountName;
            set
            {
                accountName = value;
                OnPropertyChanged();
            }
        }
        public string ZipCity 
        { 
            get => zipCity; 
            set 
            {
                zipCity = value;
                OnPropertyChanged();
            } 
        }

        [JsonIgnore]
        public IRelayCommand SaveAccountCommand { get; set; }

        public AccountViewModel()
        {
            SaveAccountCommand = new RelayCommand(SaveAccount);
        }

        private void SaveAccount()
        {
            WeakReferenceMessenger.Default.Send(new AccountChangedMessage(this));
        }

        public override int GetHashCode()
        {
            return string.IsNullOrWhiteSpace(AccountNumber) ? 0 : AccountNumber.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is AccountViewModel account)
                return account.Equals(this);
            return false;
        }

        public bool Equals(AccountViewModel? other)
        {
            if (other != null && string.Equals(this.AccountNumber, other.AccountNumber, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}
