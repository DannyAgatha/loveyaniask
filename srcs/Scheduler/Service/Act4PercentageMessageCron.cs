namespace WingsEmu.ClusterScheduler.Service;

using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Caligor;
using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;

public class Act4PercentageMessageCron : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJob.AddOrUpdate<IMessagePublisher<Act4PercentageMessage>>(
            "act4-percentage",
            s => s.PublishAsync(new Act4PercentageMessage(), CancellationToken.None),
            "*/5 * * * *", // At 10:00, 16:00, and 22:00.
            TimeZoneInfo.Utc
        );
    }
}