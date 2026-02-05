using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Quests;

namespace WingsEmu.ClusterScheduler.Service
{
    public class QuestDailyRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IMessagePublisher<QuestDailyRefreshMessage>>(
                "quest-daily-refresh",
                s => s.PublishAsync(new QuestDailyRefreshMessage
                {
                    Force = true
                }, CancellationToken.None),
                "1 0 * * *", // hardcoded cron for the moment (every day at 12am)
                TimeZoneInfo.Utc
            );
        }
    }
}