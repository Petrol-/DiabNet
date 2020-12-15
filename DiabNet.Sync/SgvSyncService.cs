using System;
using System.Collections.Generic;
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
            var dates = GenerateDates(range);
            foreach (var date in dates)
            {
                var entries = await LoadNextPoints(date);
                await TryInsertPoints(entries);
            }
        }

        private IEnumerable<DateTimeOffset> GenerateDates(DateRange range)
        {
            var start = range.From;
            var end = range.To;

            return Enumerable
                .Range(0, (end - start).Days + 1)
                .Select(day => start.AddDays(day));
        }

        private async Task<IEnumerable<Sgv>> LoadNextPoints(DateTimeOffset date)
        {
            try
            {
                return await _nightscoutApi.GetEntries(date, date);
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
