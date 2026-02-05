using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using WingsEmu.ClusterScheduler.Ranking;

namespace WingsEmu.ClusterScheduler.Service
{
    public class RankingRefreshCronScheduler : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RecurringJob.AddOrUpdate<IAsyncEventPipeline>(
                "ranking-refresh",
                s => s.ProcessEventAsync(new RankingRefreshEvent(), CancellationToken.None),
                "1 0 * * *",
                TimeZoneInfo.Utc
            );
        }
    }
}