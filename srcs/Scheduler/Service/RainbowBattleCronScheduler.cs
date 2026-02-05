using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.RainbowBattle;

namespace WingsEmu.ClusterScheduler.Service
{
    public class RainbowBattleCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<RainbowBattleStartMessage>>(
                "rainbow-battle",
                s => s.PublishAsync(new RainbowBattleStartMessage(), CancellationToken.None),
                "55 */2 * * *", // hardcoded cron for the moment
                TimeZoneInfo.Utc
            );
        }
    }
}