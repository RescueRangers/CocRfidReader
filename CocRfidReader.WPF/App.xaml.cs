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

namespace CocRfidReader.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        public App()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                        .AddJsonFile(@".\Configuration\settings.json", false, true)
                        .Build();

            services
#if DEBUG
            .AddLogging(b =>
            {
                var logger = new LoggerConfiguration()
                .WriteTo.Debug(new EcsTextFormatter(), Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.File(new EcsTextFormatter(), @".\logs\log.txt", Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day)
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
                .AddSingleton<PacklisteReader>()
                .AddTransient<MainWindow>()
                .AddSingleton<IMessagingService, WpfMessagingService>();

        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
