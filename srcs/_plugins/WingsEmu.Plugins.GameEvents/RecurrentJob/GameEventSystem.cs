using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Event;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace WingsEmu.Plugins.GameEvents.RecurrentJob
{
    public class GameEventSystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(2);
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IGameEventRegistrationManager _gameEventRegistrationManager;

        public GameEventSystem(IAsyncEventPipeline asyncEventPipeline, IGameEventRegistrationManager gameEventRegistrationManager)
        {
            _asyncEventPipeline = asyncEventPipeline;
            _gameEventRegistrationManager = gameEventRegistrationManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[GAME_EVENT_SYSTEM] Started!");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DateTime currentTime = DateTime.UtcNow;

                    foreach ((GameEventType gameEventType, DateTime expiryDate) in _gameEventRegistrationManager.GameEventRegistrations)
                    {
                        await ProcessGameEventRegistration(gameEventType, currentTime, expiryDate, stoppingToken);
                    }

                    await _asyncEventPipeline.ProcessEventAsync(new GameEventInstanceProcessEvent(currentTime), stoppingToken);
                }
                catch (Exception e)
                {
                    Log.Error("[GAME_EVENT_SYSTEM]", e);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessGameEventRegistration(GameEventType gameEventType, DateTime currentTime, DateTime expiryDate, CancellationToken cancellationToken)
        {
            if (currentTime < expiryDate)
            {
                return;
            }

            await _asyncEventPipeline.ProcessEventAsync(new GameEventLockRegistrationEvent(gameEventType), cancellationToken);
        }
    }
}