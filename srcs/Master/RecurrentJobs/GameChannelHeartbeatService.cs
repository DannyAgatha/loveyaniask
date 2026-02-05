// NosEmu
// 


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Master.Datas;
using Master.Managers;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.ServerApi;

namespace Master.RecurrentJobs
{
    public class GameChannelHeartbeatService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan LastPulseThreshold = TimeSpan.FromSeconds(30);
        private readonly IMessagePublisher<WorldServerShutdownMessage> _messagePublisher;

        private readonly WorldServerManager _worldManager;

        public GameChannelHeartbeatService(WorldServerManager worldManager, IMessagePublisher<WorldServerShutdownMessage> messagePublisher)
        {
            _worldManager = worldManager;
            _messagePublisher = messagePublisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (WorldServer world in _worldManager.GetWorlds().ToList())
                    {
                        if (world.LastPulse.AddSeconds(LastPulseThreshold.TotalSeconds) > DateTime.UtcNow)
                        {
                            Log.Debug($"[WORLD_PULSE_SYSTEM] Pulse OK : {world.WorldGroup}:{world.ChannelId}:{world.ChannelType.ToString()}");
                            // pulsed before the 10 seconds timeout
                            continue;
                        }

                        // add alert later
                        Log.Error($"[WORLD_PULSE_SYSTEM] PULSE KO - unregistering : {world.WorldGroup}:{world.ChannelId}:{world.ChannelType.ToString()}",
                            new Exception($"PULSE KO on {world.ChannelId.ToString()}"));
                        _worldManager.UnregisterWorld(world.ChannelId);
                        await _messagePublisher.PublishAsync(new WorldServerShutdownMessage
                        {
                            ChannelId = world.ChannelId
                        }, stoppingToken);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[WORLD_PULSE_SYSTEM] Unexpected error: ", e);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}