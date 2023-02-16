using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CocRfidReader.Services;
using Serilog.Formatting.Compact;
using Serilog;
using CocRfidReader.WPF.ViewModels;
using CocRfidReader.WPF.Services;
using SendGrid.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CocRfidReader.WPF.Views;
using CocRfidReader.WPF.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace CocRfidReader.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IRecipient<OpenSettingsMessage>, IRecipient<OpenAccountsMessage>, IRecipient<OpenAccountAddWindowMessage>
    {
        private IHost host;

        public App()
        {
            host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
            WeakReferenceMessenger.Default.Register<OpenSettingsMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenAccountsMessage>(this);
            WeakReferenceMessenger.Default.Register<OpenAccountAddWindowMessage>(this);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                        .AddJsonFile(@".\Configuration\settings.json", false, true)
                        .Build();

            services
#if DEBUG
            .AddLogging(b =>
            {
                var logger = new LoggerConfiguration()
                .WriteTo.Debug(new CompactJsonFormatter(), Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.File(new CompactJsonFormatter(), @".\logs\log.txt", Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day)
                .CreateLogger();

                b.AddSerilog(logger);
            })
#else
                .AddLogging(b =>
                {
                    var logger = new LoggerConfiguration()
                    .WriteTo.File(new CompactJsonFormatter(), @".\logs\log.txt", Serilog.Events.LogEventLevel.Error, rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                    b.AddSerilog(logger);
                })
#endif
                .AddSingleton<ReaderService>()
                .AddSingleton<ICocReader, CocReader>()
                .AddSingleton<ItemReader>()
                .AddSingleton<MainWindowViewModel>()
                .AddTransient<MainWindow>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<SettingsView>()
                .AddTransient<AccountsViewModel>()
                .AddTransient<AccountsView>()
                .AddTransient<AccountViewModel>()
                .AddTransient<AddAccountView>()
                .AddSingleton<CocsViewModel>()
                .AddSingleton<TagsService>()
                .AddSingleton<IMessagingService, WpfMessagingService>()
                .AddSingleton<AccountsJsonService>()
                .AddHostedService(sp => sp.GetRequiredService<AccountsJsonService>())
                .AddSingleton<ConfigurationService>()
                .AddHostedService(sp => sp.GetRequiredService<ConfigurationService>())
                .AddSendGrid(options =>
                {
                    options.ApiKey = configuration.GetValue<string>("sendGridAPI");
                });
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await host.StartAsync();
            var mainWindow = host.Services.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            var reader = host.Services.GetService<ReaderService>();
            reader.Disconnect();
        }

        void IRecipient<OpenSettingsMessage>.Receive(OpenSettingsMessage message)
        {
            var settingsView = host.Services.GetRequiredService<SettingsView>();
            settingsView.ShowDialog();
        }

        void IRecipient<OpenAccountsMessage>.Receive(OpenAccountsMessage message)
        {
            var accountsView = host.Services.GetRequiredService<AccountsView>();
            accountsView.ShowDialog();
        }

        public void Receive(OpenAccountAddWindowMessage message)
        {
            var addAccountView = host.Services.GetRequiredService<AddAccountView>();
            addAccountView.ShowDialog();
        }
    }
}
