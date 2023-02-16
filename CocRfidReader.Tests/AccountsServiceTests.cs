using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.Services;
using CocRfidReader.WPF.Services;
using CocRfidReader.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace CocRfidReader.Tests
{
    internal class AccountsServiceTests
    {
        private string accountsPath = @".\TestConfig\accounts.json";
        private IServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            var mockLogger = new Mock<ILogger<AccountsJsonService>>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILogger<AccountsJsonService>>(mockLogger.Object)
                .AddSingleton<AccountsJsonService>(new AccountsJsonService(mockLogger.Object, accountsPath))
                .AddHostedService(sp => sp.GetRequiredService<AccountsJsonService>());

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task TestReadingAccounts()
        {
            var accountService = serviceProvider.GetService<AccountsJsonService>();

            var accounts = await accountService.GetAccounts();

            Assert.That(accounts, Is.Not.Null);
            Assert.That(accounts.Count, Is.AtLeast(1));
        }

        [Test]
        public async Task TestSavingAccounts()
        {
            var accountService = serviceProvider.GetService<AccountsJsonService>();

            var accounts = new List<AccountViewModel>
            {
                new AccountViewModel
                {
                    AccountName = "Test",
                    AccountNumber = "7777",
                    ZipCity = "74-300 Myślibórz"
                }
            };

            accountService.SaveAccounts(accounts);
            var savedAccounts = await accountService.GetAccounts();

            Assert.That(savedAccounts.Count, Is.EqualTo(1));
            Assert.That(savedAccounts.First().AccountName, Is.EqualTo("Test"));
        }

        [Test]
        public async Task TestExternalEditSynchronization()
        {
            var hostedService = serviceProvider.GetService<IHostedService>();
            await hostedService.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            var accountsService = serviceProvider.GetService<AccountsJsonService>();
            var initialAccounts = await accountsService.GetAccounts();

            Assert.That(initialAccounts.Count, Is.AtLeast(2));

            var streamoptions = new FileStreamOptions
            {
                Access = FileAccess.ReadWrite,
                Mode = FileMode.Open,
                Share = FileShare.ReadWrite
            };

            using var fs = new FileStream(accountsPath, streamoptions);
            using var reader = new StreamReader(fs);
            using var writer = new StreamWriter(fs);
            
            fs.SetLength(0);


            writer.WriteLine(@"
[
  {
    ""AccountNumber"": ""7777"",
    ""AccountName"": ""Test"",
    ""ZipCity"": ""74-300 Myślibórz""
  }
]");

            writer.Flush();
            writer.Close();

            reader.Dispose();
            reader.Close();
            fs.Close();
            fs.Dispose();

            await Task.Delay(500);
            var changedAccounts = await accountsService.GetAccounts();
            Assert.That(changedAccounts.Count, Is.EqualTo(1));
            Assert.That(changedAccounts.First().AccountName, Is.EqualTo("Test"));
        }
    }
}
