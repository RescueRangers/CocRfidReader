using System.Collections.Generic;
using System.Threading.Tasks;
using CocRfidReader.WPF.ViewModels;

namespace CocRfidReader.WPF.Services
{
    public interface IAccountsService
    {
        Task<IEnumerable<AccountViewModel>> GetAccounts();
    }
}