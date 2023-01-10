using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CocRfidReader.WPF.ViewModels;

namespace CocRfidReader.WPF.Services
{
    public interface IAccountsService
    {
        event EventHandler AccountsChanged;

        Task<IEnumerable<AccountViewModel>> GetAccounts();
    }
}