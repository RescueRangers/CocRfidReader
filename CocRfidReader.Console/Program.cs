// See https://aka.ms/new-console-template for more information

using CocRfidReader.Model;
using CocRfidReader.Services;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

internal class Program
{
    private static ConcurrentObservableDictionary<string, Coc?> cocs = new ConcurrentObservableDictionary<string, Coc?>();
    private static ConcurrentObservableDictionary<string, Tag> tags = new ConcurrentObservableDictionary<string, Tag>();
    private static ConcurrentObservableDictionary<string, TagItem?> items = new ConcurrentObservableDictionary<string, TagItem?>();
    private static ILogger<Program>? logger;

    private static CocReader cocReader;
    private static ItemReader itemReader;

    private static void Main(string[] args)
    {
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
            .AddSingleton<ReaderService>()
            .AddSingleton<CocReader>()
            .AddSingleton<ItemReader>()
            .AddSingleton<ConfigurationService>()
            .BuildServiceProvider();

        var reader = serviceProvider.GetRequiredService<ReaderService>().Connect();

        reader.TagsReported += Reader_TagsReported;
        tags.CollectionChanged += Tags_CollectionChanged;

        cocReader = serviceProvider.GetRequiredService<CocReader>();
        itemReader = serviceProvider.GetRequiredService<ItemReader>();
        reader.ReaderStopped += Reader_ReaderStopped;


        Console.WriteLine("Press \"R\" to start reading");

        var key = Console.ReadKey();

        if (key.Key == ConsoleKey.R)
        {
            reader.Start();
        }


        while (true)
        {
            Console.ReadKey();
        }
    }

    private static void Reader_ReaderStopped(ImpinjReader reader, ReaderStoppedEvent e)
    {
        Console.WriteLine("Scanned Cocs:");
        foreach (var coc in cocs)
        {
            Console.WriteLine(coc.Value);
        }
        Console.WriteLine("Scanned Items");
        foreach (var item in items)
        {
            Console.WriteLine(item.Value);
        }
    }

    async static void Tags_CollectionChanged(object? sender, DictionaryChangedEventArgs<string, Tag> e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            try
            {
                var epcValue = e.NewValue.Epc.ToHexString();
                Console.WriteLine($"Tag added with epc: {e.NewValue.Epc}");
                if (epcValue.StartsWith("888"))
                {
                    logger?.LogInformation("Getting COC from the database");
                    var coc = await cocReader.GetAsync(epcValue);

                    if (coc == null) return;

                    logger?.LogInformation($"Database returned {coc.PRODUKTIONSNR}");
                    cocs.TryAdd(epcValue, coc);
                }
                else if (epcValue.StartsWith("777"))
                {
                    logger?.LogInformation("Getting Item from the database");
                    var item = await itemReader.GetAsync(epcValue);

                    if (item == null) return;

                    logger?.LogInformation($"Database returned {item.ItemNumber}");
                    items.TryAdd(epcValue, item);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }

        }
    }

    static void Reader_TagsReported(ImpinjReader reader, TagReport report)
    {
        foreach (Tag tag in report)
        {
            Console.WriteLine($"EPC: {tag.Epc}");
            tags.TryAdd(tag.Epc.ToString(), tag);
        }
    }
}

