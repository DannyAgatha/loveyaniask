using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Rewards;

namespace WingsEmu.ClusterScheduler.Service;

public class RewardRestrictionRefreshCronScheduler : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<IMessagePublisher<RewardRestrictionRefreshMessage>>(
            "reward-restriction-refresh",
            s => s.PublishAsync(new RewardRestrictionRefreshMessage(), CancellationToken.None),
            "1 0 * * *", // hardcoded cron for the moment
            TimeZoneInfo.Utc
        );
    }
}