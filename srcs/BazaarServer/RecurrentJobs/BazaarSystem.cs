using System;
using System.Threading;
using System.Threading.Tasks;
using BazaarServer.Managers;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;

namespace BazaarServer.RecurrentJobs
{
    public class BazaarSystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
        private readonly BazaarManager _bazaarManager;

        public BazaarSystem(BazaarManager bazaarManager) => _bazaarManager = bazaarManager;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[BAZAAR_SYSTEM] Caching all items in the database...");
            long countCachedItems = await _bazaarManager.CacheAllItemsInDb();
            Log.Info($"[BAZAAR_SYSTEM] Cached: {countCachedItems.ToString()} item/s");
            Log.Info("[BAZAAR_SYSTEM] Started!");
        }
    }
}