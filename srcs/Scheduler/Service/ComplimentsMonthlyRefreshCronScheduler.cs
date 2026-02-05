using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Compliments;

namespace WingsEmu.ClusterScheduler.Service
{
    public class ComplimentsMonthlyRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<ComplimentsMonthlyRefreshMessage>>(
                "compliments-monthly-refresh",
                s => s.PublishAsync(new ComplimentsMonthlyRefreshMessage(), CancellationToken.None),
                "1 0 1 * *", // hardcoded cron for the moment (every first of the month at 12am)
                TimeZoneInfo.Utc
            );
        }
    }
}