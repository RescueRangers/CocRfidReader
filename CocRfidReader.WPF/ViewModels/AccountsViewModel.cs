using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.WPF.ViewModels
{
    public class AccountsViewModel : ObservableObject
    {
        private AccountsJsonService accountsService;
        private ILogger<AccountsViewModel> logger;

        public IRelayCommand DeleteAccountCommand { get; set; }
        public IRelayCommand ClosingAccounts { get; set; }
        public IRelayCommand AddAccountCommand { get; set; }

        public AccountsViewModel(AccountsJsonService accountsService)
        {
            this.accountsService = accountsService;
            GetAccounts();

            DeleteAccountCommand = new RelayCommand<AccountViewModel>(account => DeleteAccount(account));
            ClosingAccounts = new RelayCommand(Closing);
            AddAccountCommand = new RelayCommand(AddAccount);
        }

        private void AddAccount()
        {
            Accounts.Add(new AccountViewModel());
        }

        private void Closing()
        {
            accountsService.SaveAccounts(Accounts);
        }

        private void DeleteAccount(AccountViewModel account)
        {
            Accounts.Remove(account);
        }

        private async void GetAccounts()
        {
            try
            {
                Accounts = new ObservableCollection<AccountViewModel>(await accountsService.GetAccounts());
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        public ObservableCollection<AccountViewModel> Accounts { get; set; }
    }
}
