using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Miniland;

namespace WingsEmu.ClusterScheduler.Service
{
    public class MinigameProductionRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<MinigameRefreshProductionPointsMessage>>(
                "minigame-refresh-free-production-points",
                s => s.PublishAsync(new MinigameRefreshProductionPointsMessage(), CancellationToken.None),
                "1 0 * * *", // hardcoded cron for the moment
                TimeZoneInfo.Utc
            );
        }
    }
}