using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiabNet.Domain;
using DiabNet.Domain.Services;
using DiabNet.Nightscout;
using Microsoft.Extensions.Logging;

namespace DiabNet.Sync
{
    public class SgvSyncService : ISgvSyncService
    {
        private readonly INightscoutApi _nightscoutApi;
        private readonly ISearchService _searchService;
        private readonly ILogger<SgvSyncService> _log;

        public SgvSyncService(ILogger<SgvSyncService> log, INightscoutApi nightscoutApi, ISearchService searchService)
        {
            _nightscoutApi = nightscoutApi;
            _searchService = searchService;
            _log = log;
        }

        public async Task Synchronize(DateRange range)
        {
            var ranges = range.SplitByDay(3);
            var total = 0;
            Stopwatch executionTime = Stopwatch.StartNew();
            foreach (var slice in ranges)
            {
                var entries = await LoadNextPoints(slice);
                var count = entries.Count;
                total += count;
                _log.LogInformation($"Got {count} entries for range {FormatDate(slice.From)} -> {FormatDate(slice.To)}");
                await TryInsertPoints(entries);
            }
            _log.LogInformation($"Synchronization finished: {total} entries from {FormatDate(range.From)} to {FormatDate(range.To)} (took {executionTime.Elapsed:g})");
        }

        private string FormatDate(DateTimeOffset date) => $"{date:yyyy MMMM dd}";

        private async Task<IList<Sgv>> LoadNextPoints(DateRange range)
        {
            try
            {
                var entries = await _nightscoutApi.GetEntries(range.From, range.To);
                return entries.ToList();
            }
            catch (Exception e)
            {
                throw new SyncException("Failed to load entries from Nightscout", e);
            }
        }

        private async Task TryInsertPoints(IEnumerable<Sgv> points)
        {
            foreach (var p in points)
            {
                if (p == null) continue;
                await InsertPoint(p);
            }
        }

        private async Task InsertPoint(Sgv point)
        {
            try
            {
                await _searchService.InsertSgvPoint(point);
            }
            catch (Exception e)
            {
                _log.LogWarning("Could not insert point {point} : {e}", point, e);
            }
        }
    }
}
