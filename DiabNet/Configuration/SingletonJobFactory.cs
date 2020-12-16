using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace DiabNet.Configuration
{
    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var job = _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;

            if (job == null)
                throw new InvalidOperationException($"No job found with type {nameof(bundle.JobDetail.JobType)}");
            return job;
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
