using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Item;

namespace WingsEmu.ClusterScheduler.Service
{
    public class ItemRestrictionRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<ItemRestrictionRefreshMessage>>(
                "item-restriction-refresh",
                s => s.PublishAsync(new ItemRestrictionRefreshMessage(), CancellationToken.None),
                "1 0 * * *", // hardcoded cron for the moment
                TimeZoneInfo.Utc
            );
        }
    }
}