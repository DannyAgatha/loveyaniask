using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.WorldBoss;

namespace WingsEmu.ClusterScheduler.Service
{
    public class WorldBossDailyLotteryScheduler : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<WorldBossDailyLotteryMessage>>(
                "worldboss-daily-lottery",
                s => s.PublishAsync(new WorldBossDailyLotteryMessage
                {
                    Force = true
                }, CancellationToken.None),
                "0 0 * * *", // todos los días a medianoche UTC
                TimeZoneInfo.Utc
            );

            return Task.CompletedTask;
        }
    }
}