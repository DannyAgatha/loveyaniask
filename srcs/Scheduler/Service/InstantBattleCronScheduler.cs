using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.InstantBattle;

namespace WingsEmu.ClusterScheduler.Service
{
    public class InstantBattleCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<InstantBattleStartMessage>>(
                "instant-battle",
                s => s.PublishAsync(new InstantBattleStartMessage(), CancellationToken.None),
                "55 1-23/2 * * *", // hardcoded cron for the moment
                TimeZoneInfo.Utc
            );
        }
    }
}