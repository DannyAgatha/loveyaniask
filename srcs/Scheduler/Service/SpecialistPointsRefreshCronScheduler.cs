using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Player;

namespace WingsEmu.ClusterScheduler.Service
{
    public class SpecialistPointsRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<SpecialistPointsRefreshMessage>>(
                "specialist-points-refresh",
                s => s.PublishAsync(new SpecialistPointsRefreshMessage(), CancellationToken.None),
                "1 0 * * *", // hardcoded cron for the moment
                TimeZoneInfo.Utc
            );
        }
    }
}