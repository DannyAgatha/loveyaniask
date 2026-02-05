using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Alzanor.Communication;

namespace WingsEmu.ClusterScheduler.Service;

public class AlzanorCronScheduler : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<IMessagePublisher<AlzanorStartMessage>>(
            "alzanor",
            s => s.PublishAsync(new AlzanorStartMessage(), CancellationToken.None),
            "30 */2 * * *",
            TimeZoneInfo.Utc
        );
    }
}