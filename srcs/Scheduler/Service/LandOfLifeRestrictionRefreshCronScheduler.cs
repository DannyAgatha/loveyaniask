using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.LandOfLife;

namespace WingsEmu.ClusterScheduler.Service;

public class LandOfLifeRestrictionRefreshCronScheduler : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<IMessagePublisher<LandOfLifeRestrictionRefreshMessage>>(
            "land-of-life-restriction-refresh",
            s => s.PublishAsync(new LandOfLifeRestrictionRefreshMessage(), CancellationToken.None),
            "1 0 * * *", // every day at 00:01 UTC
            TimeZoneInfo.Utc
        );
    }
}