using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
namespace DiabNet.Features
{
    [DisallowConcurrentExecution]
    public class SyncSgvJob : IJob
    {
        private readonly ILogger<SyncSgvJob> _log;

        public SyncSgvJob(ILogger<SyncSgvJob> log)
        {
            _log = log;
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
