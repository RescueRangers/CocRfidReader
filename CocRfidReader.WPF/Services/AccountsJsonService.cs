using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CocRfidReader.WPF.Models;
using CocRfidReader.WPF.ViewModels;

namespace CocRfidReader.WPF.Services
{
    public class AccountsJsonService : IAccountsService
    {
        public async Task<IEnumerable<AccountViewModel>> GetAccounts()
        {
            var file = new FileInfo(@".\Configuration\accounts.json");
            using (var reader = file.OpenText())
            {
                var json = reader.ReadToEnd();
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                var result = JsonSerializer.DeserializeAsyncEnumerable<AccountViewModel>(stream);
                return await result.ToListAsync();
            }
        }
    }
}
