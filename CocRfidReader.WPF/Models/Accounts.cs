using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.WPF.ViewModels;

namespace CocRfidReader.WPF.Models
{
    public class DeserializableAccounts
    {
        public IAsyncEnumerable<AccountViewModel> Accounts { get; set; }
    }
}
