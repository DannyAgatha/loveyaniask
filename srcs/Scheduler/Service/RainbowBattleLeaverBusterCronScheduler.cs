using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.RainbowBattle;

namespace WingsEmu.ClusterScheduler.Service
{
    public class RainbowBattleLeaverBusterCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<RainbowBattleLeaverBusterResetMessage>>(
                "rainbow-battle-leaver-buster-reset",
                s => s.PublishAsync(new RainbowBattleLeaverBusterResetMessage
                {
                    Force = true
                }, CancellationToken.None),
                "1 0 1 * *",
                TimeZoneInfo.Utc
            );
        }
    }
}