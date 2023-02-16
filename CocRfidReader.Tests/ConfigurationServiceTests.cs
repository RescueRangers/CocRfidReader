using CocRfidReader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace CocRfidReader.Tests
{
    public class ConfigurationServiceTests
    {
        private string configurationPath = @".\TestConfig\settings.json";
        private IServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            var mockLogger = new Mock<ILogger<ConfigurationService>>();
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ILogger<ConfigurationService>>
                    (mockLogger.Object)
                .AddSingleton<ConfigurationService>
                    (
                        new ConfigurationService(mockLogger.Object, 
                        configurationPath
                    ))
                .AddHostedService
                    (sp => sp.GetRequiredService<ConfigurationService>());

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public void TestReadingConfiguration()
        {
            var configurationService = serviceProvider
                .GetService<ConfigurationService>();

            var initialConfig = configurationService.GetSettings();

            Assert.IsNotNull(initialConfig);
        }

        [Test]
        public void TestWritingConfiguration()
        {
            var configurationService = serviceProvider.GetService<ConfigurationService>();

            var initialConfig = configurationService.GetSettings();
            initialConfig.Antena1Power = 30;
            configurationService.SaveSettings(initialConfig);

            var changedConfig = configurationService.GetSettings();
            Assert.That(changedConfig.Antena1Power, Is.EqualTo(30));
        }

        [Test]
        public async Task TestExternalEditSynchronization()
        {
            var hostedService = serviceProvider
                .GetService<IHostedService>();
            await hostedService
                .StartAsync(CancellationToken.None);
            await Task.Delay(500);

            var configurationService = serviceProvider
                .GetService<ConfigurationService>();

            var streamOptions = new FileStreamOptions
            {
                Access = FileAccess.ReadWrite,
                Mode = FileMode.Open,
                Share = FileShare.ReadWrite
            };

            using var fs = new FileStream(configurationPath, streamOptions);
            using var reader = new StreamReader(fs);
            using var writer = new StreamWriter(fs);

            List<string> lines = reader
                .ReadToEnd()
                .Split
                (
                    new string[] {"\r\n"},
                    StringSplitOptions.None
                ).ToList();

            var index = lines
                .FindIndex(s => s.StartsWith("  \"antena1Power\": 20,"));

            lines[index] = "  \"antena1Power\": 30,";

            fs.SetLength(0);

            writer.Write(string.Join("\r\n", lines));

            writer.Flush();
            writer.Close();
            reader.Dispose();
            reader.Close();
            fs.Close();
            fs.Dispose();

            await Task.Delay(500);
            var changedConfig = configurationService.GetSettings();
            Assert.That(changedConfig.Antena1Power, Is.EqualTo(30));
        }
    }
}