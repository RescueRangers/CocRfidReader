using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.WPF.ViewModels
{
    public class AccountsViewModel : ObservableObject
    {
        private AccountsJsonService accountsService;
        private ILogger<AccountsViewModel> logger;

        public AccountsViewModel(AccountsJsonService accountsService)
        {
            this.accountsService = accountsService;
            GetAccounts();
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
