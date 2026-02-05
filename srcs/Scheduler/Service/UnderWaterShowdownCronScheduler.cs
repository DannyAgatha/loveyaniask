using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.UnderWaterShowDown;

namespace WingsEmu.ClusterScheduler.Service;

public class UnderWaterShowdownCronScheduler : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<IMessagePublisher<UnderWaterShowdownStartMessage>>(
            "underwater-showdown",
            s => s.PublishAsync(new UnderWaterShowdownStartMessage(), CancellationToken.None),
            "25 */3 * * *", // hardcoded cron for the moment
            TimeZoneInfo.Utc
        );
    }
}