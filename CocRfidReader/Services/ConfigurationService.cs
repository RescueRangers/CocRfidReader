using System;
using System.Text;
using System.Text.Json;
using CocRfidReader.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.Services
{
    public class ConfigurationService : BackgroundService
    {
        private FileSystemWatcher? watcher;
        private ILogger<ConfigurationService> logger;
        private bool fileIsChanging = false;
        private SettingsModel? settings;

        public event EventHandler SettingsChanged;

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            this.logger = logger;
        }

        public SettingsModel GetSettings()
        {
            if (settings != null) return settings;
            var file = new FileInfo(@".\Configuration\settings.json");

            using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream, Encoding.UTF8);

            var json = reader.ReadToEnd();
            settings = JsonSerializer.Deserialize<SettingsModel>(json);
            reader.Close();
            fileStream.Close();
            return settings;
        }

        public void SaveSettings(SettingsModel settings)
        {
            var file = new FileInfo(@".\Configuration\settings.json");
            var json = JsonSerializer.Serialize(settings);

            using var fileStream = new FileStream(file.FullName, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);

            writer.WriteLine(json);
            writer.Close();
            fileStream.Close();
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
            watcher = new FileSystemWatcher(@".\Configuration\", "settings.json");
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Size;

            watcher.Changed += Configuration_Changed;
        }

        private void Configuration_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (fileIsChanging) return;
                var file = new FileInfo(@".\Configuration\settings.json");

                using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);

                var json = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(json)) return;
                fileIsChanging = true;
                settings = JsonSerializer.Deserialize<SettingsModel>(json);
                reader.Close();
                fileStream.Close();
                fileIsChanging = false;
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
