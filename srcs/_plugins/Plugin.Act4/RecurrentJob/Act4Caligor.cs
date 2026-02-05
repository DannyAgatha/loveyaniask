using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Act4;

namespace Plugin.Act4.RecurrentJob
{
    public class Act4Caligor : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

        private readonly IAct4CaligorManager _manager;
        public Act4Caligor(IAct4CaligorManager manager)
        {
            _manager = manager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[ACT4_CALIGOR_SYSTEM] Started!");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DateTime currentTime = DateTime.UtcNow;
                    await Process(currentTime, stoppingToken);
                }
                catch (Exception e)
                {
                    Log.Error("[ACT4_CALIGOR_SYSTEM]", e);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task Process(DateTime time, CancellationToken stoppingToken)
        {
            if (!_manager.CaligorActive)
            {
                return;
            }

            _manager.RefreshCaligorInstance();

            if (_manager.CaligorEnd >= time)
            {
                return;
            }

            _manager.EndCaligorInstance(false);
        }
    }
}