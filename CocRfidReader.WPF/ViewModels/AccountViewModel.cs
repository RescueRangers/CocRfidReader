using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CocRfidReader.WPF.Messages;
using CocRfidReader.WPF.Validators;
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

        private AccountValidator validator = new AccountValidator();

        [Required]
        public string AccountNumber
        {
            get => accountNumber;
            set
            {
                accountNumber = value;
                OnPropertyChanged();
                SaveAccountCommand.NotifyCanExecuteChanged();
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
            SaveAccountCommand = new RelayCommand(SaveAccount, CanSave);
        }

        private bool CanSave()
        {
            return validator.Validate(this).IsValid;
        }

        private void SaveAccount()
        {
            WeakReferenceMessenger.Default.Send(new AccountChangedMessage(this));
        }

        public string this[string columnName]
        {
            get
            {
                var firstOrDefault = validator.Validate(this).Errors.FirstOrDefault(p => p.PropertyName == columnName);
                if (firstOrDefault != null)
                    return validator != null ? firstOrDefault.ErrorMessage : "";
                return "";
            }
        }

        public string Error
        {
            get
            {
                if (validator != null)
                {
                    var results = validator.Validate(this);
                    if (results != null && results.Errors.Any())
                    {
                        var errors = string.Join(Environment.NewLine, results.Errors.Select(x => x.ErrorMessage).ToArray());
                        return errors;
                    }
                }
                return string.Empty;
            }
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

        public override string ToString()
        {
            return $"{AccountNumber} | {AccountName} | {ZipCity}";
        }
    }
}
