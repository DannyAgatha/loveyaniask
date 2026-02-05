using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Icebreaker;
using WingsAPI.Communication.RainbowBattle;

namespace WingsEmu.ClusterScheduler.Service;

public class IcebreakerCronScheduler : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<IMessagePublisher<IcebreakerStartMessage>>(
            "icebreaker",
            s => s.PublishAsync(new IcebreakerStartMessage(), CancellationToken.None),
            "44 23 * * *", // hardcoded cron for the moment
            TimeZoneInfo.Utc
        );
    }
}