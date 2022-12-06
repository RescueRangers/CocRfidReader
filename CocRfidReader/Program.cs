using System.Collections.Concurrent;
using CocRfidReader.Services;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Compact;

namespace CocRfidReader
{
    internal class Program
    {
        private static ConcurrentObservableDictionary<string, Tag> tags = new ConcurrentObservableDictionary<string, Tag>();

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(@".\Configuration\settings.json", false, true)
                .Build();

            var serviceProvider = new ServiceCollection()
#if DEBUG
                .AddLogging(b =>
                {
                    var logger = new LoggerConfiguration()
                    .WriteTo.Console(new CompactJsonFormatter(), Serilog.Events.LogEventLevel.Verbose)
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
                .AddSingleton<IConfiguration>(builder)
                .AddSingleton<ReaderService>()
                .BuildServiceProvider();

            var reader = serviceProvider.GetRequiredService<ReaderService>().Reader;

            reader.TagsReported += Reader_TagsReported;
            tags.CollectionChanged += Tags_CollectionChanged;

            Console.WriteLine("Press \"R\" to start readind");

            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.R)
            {
                reader.Start();
            }

            while (true)
            {
                Console.ReadKey();
            }
            Console.WriteLine("Hello, World!");
        }

        private static void Tags_CollectionChanged(object? sender, DictionaryChangedEventArgs<string, Tag> e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Console.WriteLine($"Tag added with epc: {e.NewValue.Epc}");
            }
        }

        private static void Reader_TagsReported(ImpinjReader reader, TagReport report)
        {
            foreach (Tag tag in report)
            {
                Console.WriteLine($"EPC: {tag.Epc}");
                tags.TryAdd(tag.Epc.ToString(), tag);
            }
        }
    }
}