using System.Collections.Concurrent;
using CocRfidReader.Model;
using CocRfidReader.Services;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Formatting.Compact;

namespace CocRfidReader
{
    internal class Program
    {
        private static ConcurrentObservableDictionary<string, Tag> tags = new ConcurrentObservableDictionary<string, Tag>();
        private static ConcurrentObservableDictionary<string, Coc?> cocs = new ConcurrentObservableDictionary<string, Coc?>();
        private static ConcurrentObservableDictionary<string, TagItem?> items = new ConcurrentObservableDictionary<string, TagItem?>();
        private static ILogger<Program> logger;

        private static CocReader cocReader;
        private static ItemReader itemReader;

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
                .AddSingleton<CocReader>()
                .AddSingleton<ItemReader>()
                .BuildServiceProvider();

            var reader = serviceProvider.GetRequiredService<ReaderService>().Reader;
            cocReader = serviceProvider.GetRequiredService<CocReader>();
            itemReader = serviceProvider.GetRequiredService<ItemReader>();
            logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            reader.TagsReported += Reader_TagsReported;
            //tags.CollectionChanged += Tags_CollectionChanged;

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

        private async static void Tags_CollectionChanged(object? sender, DictionaryChangedEventArgs<string, Tag> e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                try
                {
                    var epcValue = e.NewValue.Epc.ToHexString();
                    Console.WriteLine($"Tag added with epc: {e.NewValue.Epc}");
                    if (epcValue.StartsWith("888"))
                    {
                        logger.LogInformation("Getting COC from the database");
                        var coc = await cocReader.GetAsync(epcValue);

                        if (coc == null) return;

                        logger.LogInformation($"Database returned {coc.PRODUKTIONSNR}");
                        cocs.TryAdd(epcValue, coc);
                        Console.WriteLine($"Added COC: {coc.PRODUKTIONSNR}");
                    }
                    else if (epcValue.StartsWith("777"))
                    {
                        logger.LogInformation("Getting Item from the database");
                        var item = await itemReader.GetAsync(epcValue);

                        if (item == null) return;

                        logger.LogInformation($"Database returned {item.ItemNumber}");
                        items.TryAdd(epcValue, item);
                        Console.WriteLine($"Added item: {item.ItemNumber}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    throw;
                }
                
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