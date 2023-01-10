using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CocRfidReader.Services;
using Serilog.Formatting.Compact;
using Serilog;
using CocRfidReader.WPF.ViewModels;
using CocRfidReader.WPF.Services;
using Elastic.CommonSchema.Serilog;
using SendGrid.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CocRfidReader.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
            //ServiceCollection services = new();
            //ConfigureServices(services);
            //serviceProvider = services.BuildServiceProvider();
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
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<ReaderService>()
                .AddSingleton<CocReader>()
                .AddSingleton<ItemReader>()
                .AddSingleton<MainWindowViewModel>()
                .AddTransient<MainWindow>()
                .AddSingleton<CocsViewModel>()
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
    }
}
