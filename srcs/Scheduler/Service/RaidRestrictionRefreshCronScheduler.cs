using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Raid;

namespace WingsEmu.ClusterScheduler.Service
{
    public class RaidRestrictionRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<RaidRestrictionRefreshMessage>>(
                "raid-restriction-refresh",
                s => s.PublishAsync(new RaidRestrictionRefreshMessage(), CancellationToken.None),
                "1 0 * * *", // hardcoded cron for the moment
                TimeZoneInfo.Utc
            );
        }
    }
}