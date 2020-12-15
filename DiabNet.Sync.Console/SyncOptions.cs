using System;
using CommandLine;

namespace DiabNet.Sync.Console
{
    public class SyncOptions
    {
        public SyncOptions(DateTimeOffset from, DateTimeOffset to, string elasticUrl, string nightscoutUrl)
        {
            From = from;
            To = to;
            ElasticUrl = elasticUrl;
            NightscoutUrl = nightscoutUrl;
        }

        [Option('f', "from", Required = true, HelpText = "When to start")]
        public DateTimeOffset From { get; }

        [Option('t', "to", Required = true, HelpText = "When to stop")]
        public DateTimeOffset To { get; }

        [Option('e', "elastic", Required = true, HelpText = "ElasticSearch url")]
        public string ElasticUrl { get; }

        [Option('n', "nightscout", Required = true, HelpText = "Nightscout url")]
        public string NightscoutUrl { get; }
    }
}
