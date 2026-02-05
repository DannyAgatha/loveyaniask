using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Families;

namespace WingsEmu.ClusterScheduler.Service
{
    public class FamilyMissionsResetCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<FamilyMissionsResetMessage>>(
                "family-missions-reset",
                s => s.PublishAsync(new FamilyMissionsResetMessage(), CancellationToken.None),
                "1 0 * * *",
                TimeZoneInfo.Utc
            );
        }
    }
}