using System;
using System.Threading.Tasks;
using CommandLine;
using DiabNet.Domain.Services;
using DiabNet.ElasticSearch;
using DiabNet.Nightscout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using DateRange = DiabNet.Domain.DateRange;

namespace DiabNet.Sync.Console
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await Parser
                .Default
                .ParseArguments<SyncOptions>(args)
                .MapResult(async options =>
                        await ConfigureAndStart(options),
                    _ => Task.FromResult(0)
                );

            System.Console.WriteLine("done");
        }

        private static async Task ConfigureAndStart(SyncOptions options)
        {
            var services = new ServiceCollection();
            services.AddSingleton(new DateRange(options.From, options.To));
            services.AddLogging(l => l
                .AddDebug()
                .AddConsole());
            services.AddSingleton(new ElasticClient(new Uri(options.ElasticUrl)));
            services.AddHttpClient<INightscoutApi, NightscoutApi>(c =>
            {
                c.BaseAddress = new Uri(options.NightscoutUrl);
                c.Timeout = TimeSpan.FromSeconds(5);
            });
            services.AddTransient<ISearchService, ElasticSearchService>();
            services.AddTransient<ISgvSyncService, SgvSyncService>();
            var serviceProvider = services.BuildServiceProvider();

            await Start(serviceProvider);
        }

        private static async Task Start(IServiceProvider services)
        {
            var syncService = services.GetService<ISgvSyncService>() ??
                              throw new ArgumentNullException(nameof(ISgvSyncService));
            var dateRange = services.GetService<DateRange>() ??
                            throw new ArgumentNullException(nameof(DateRange));

            await syncService.Synchronize(dateRange);
        }
    }
}
