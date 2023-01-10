using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CocRfidReader.WPF.Models;
using CocRfidReader.WPF.ViewModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.WPF.Services
{
    public class AccountsJsonService : BackgroundService, IAccountsService
    {
        private FileSystemWatcher? watcher;
        private IEnumerable<AccountViewModel>? accounts;
        private ILogger<AccountsJsonService> logger;
        private bool fileIsChanging = false;

        public event EventHandler AccountsChanged;

        public AccountsJsonService(ILogger<AccountsJsonService> logger)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<AccountViewModel>> GetAccounts()
        {
            if (accounts != null) return accounts;
            var file = new FileInfo(@".\Configuration\accounts.json");

            using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream, Encoding.UTF8);

            var json = await reader.ReadToEndAsync();
            fileIsChanging = true;
            accounts = JsonSerializer.Deserialize<List<AccountViewModel>>(json);
            reader.Close();
            fileStream.Close();
            return accounts;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (watcher == null) ConfigureWatcher();
                await Task.Delay(20000);
            }
        }

        private void ConfigureWatcher()
        {
            watcher = new FileSystemWatcher(@".\Configuration\", "accounts.json");
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Size;

            watcher.Changed += Settings_Changed;
        }

        private void Settings_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (fileIsChanging) return;
                var file = new FileInfo(@".\Configuration\accounts.json");

                using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);

                var json = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(json)) return;
                fileIsChanging = true;
                accounts = JsonSerializer.Deserialize<List<AccountViewModel>>(json);
                reader.Close();
                fileStream.Close();
                fileIsChanging = false;
                AccountsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
