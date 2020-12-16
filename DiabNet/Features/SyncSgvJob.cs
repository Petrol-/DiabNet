using System;
using System.Threading.Tasks;
using DiabNet.Domain;
using DiabNet.Domain.Services;
using Microsoft.Extensions.Logging;
using Quartz;
namespace DiabNet.Features
{
    [DisallowConcurrentExecution]
    public class SyncSgvJob : IJob
    {
        private readonly ILogger<SyncSgvJob> _log;
        private readonly ISgvSyncService _syncService;

        public SyncSgvJob(ILogger<SyncSgvJob> log, ISgvSyncService syncService)
        {
            _log = log;
            _syncService = syncService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _log.LogInformation($"Synchronisation job started");
            try
            {
                await _syncService.Synchronize(GenerateDateRangeOneHour());
            }
            catch (Exception e)
            {
                _log.LogError(e, "Synchronisation error");
            }
            _log.LogInformation($"Synchronisation job finished");
        }

        private DateRange GenerateDateRangeOneHour()
        {
            var now = DateTimeOffset.Now;
            return new DateRange(now.AddHours(-1), now);
        }
    }
}
