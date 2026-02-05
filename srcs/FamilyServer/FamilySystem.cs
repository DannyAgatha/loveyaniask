using System;
using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Managers;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;

namespace FamilyServer
{
    public class FamilySystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.FamilyServerSaveIntervalMinutes) ?? "5"));

        private readonly IFamilyWarehouseManager _familyWarehouseManager;

        public FamilySystem(IFamilyWarehouseManager familyWarehouseManager) => _familyWarehouseManager = familyWarehouseManager;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[FAMILY_SYSTEM] Loaded");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessMain();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        public async Task ProcessMain()
        {
            await _familyWarehouseManager.FlushWarehouseSaves();
        }
    }
}