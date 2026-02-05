using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Caligor;

namespace WingsEmu.ClusterScheduler.Service
{
    public class Act4CaligorCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<CaligorStartMessage>>(
                "caligor",
                s => s.PublishAsync(new CaligorStartMessage(), CancellationToken.None),
                "0 16,20,0 * * *", // At 16:00, 20:00, and 00:00.
                TimeZoneInfo.Utc
            );
        }
    }
    
}