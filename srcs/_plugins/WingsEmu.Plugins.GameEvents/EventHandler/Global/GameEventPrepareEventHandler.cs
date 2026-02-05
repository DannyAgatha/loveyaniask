using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace WingsEmu.Plugins.GameEvents.EventHandler.Global
{
    public class GameEventPrepareEventHandler : IAsyncEventProcessor<GameEventPrepareEvent>
    {
        private static readonly Step[] Steps =
        {
            new(new TimeSpan(0, 0, 0), 5, false),
            new(new TimeSpan(0, 2, 0), 3, false),
            new(new TimeSpan(0, 4, 0), 1, false),
            new(new TimeSpan(0, 4, 30), 30, true),
            new(new TimeSpan(0, 4, 50), 10, true)
        };

        private static readonly TimeSpan UnlockDelayAfterLastStep = TimeSpan.FromSeconds(10);
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IScheduler _scheduler;

        private readonly SerializableGameServer _serializableGameServer;
        private readonly ISessionManager _sessionManager;

        public GameEventPrepareEventHandler(ISessionManager sessionManager, IGameLanguageService gameLanguageService, IScheduler scheduler, IAsyncEventPipeline eventPipeline,
            SerializableGameServer serializableGameServer)
        {
            _sessionManager = sessionManager;
            _gameLanguageService = gameLanguageService;
            _scheduler = scheduler;
            _eventPipeline = eventPipeline;
            _serializableGameServer = serializableGameServer;
        }

        public async Task HandleAsync(GameEventPrepareEvent e, CancellationToken cancellation)
        {
            if (_serializableGameServer.ChannelType == GameChannelType.ACT_4)
            {
                return;
            }

            GameEventType type = e.Type;
            Log.Debug("[GameEvent] Preparing Event: " + type);

            if (e.NoDelay)
            {
                switch (type)
                {
                    case GameEventType.InstantBattle:
                        await _eventPipeline.ProcessEventAsync(new GameEventUnlockRegistrationEvent(type));
                        break;
                }

                return;
            }

            for (int i = 0; i < Steps.Length; i++)
            {
                Step step = Steps[i];

                int local = i;
                GameDialogKey isSecMessage = step.IsSecond ? GameDialogKey.GAMEEVENT_SHOUTMESSAGE_PREPARATION_SECONDS : GameDialogKey.GAMEEVENT_SHOUTMESSAGE_PREPARATION_MINUTES;

                _scheduler.Schedule(step.Delay, () =>
                {
                    switch (type)
                    {
                        case GameEventType.InstantBattle:
                            _sessionManager.BroadcastAsync(async x =>
                            {
                                string instantBattleName = _gameLanguageService.GetLanguage(GameDialogKey.INSTANT_COMBAT_NAME, x.UserLanguage);
                                string message = _gameLanguageService.GetLanguageFormat(isSecMessage, x.UserLanguage, instantBattleName, step.DisplayTime);

                                return x.GenerateMsgPacket(message, MsgMessageType.Middle);
                            });

                            _sessionManager.BroadcastAsync(async x =>
                            {
                                string instantBattleName = _gameLanguageService.GetLanguage(GameDialogKey.INSTANT_COMBAT_NAME, x.UserLanguage);
                                string message = _gameLanguageService.GetLanguageFormat(isSecMessage, x.UserLanguage, instantBattleName, step.DisplayTime);

                                return x.GenerateMsgPacket(string.Format(message, step.DisplayTime), MsgMessageType.BottomCard);
                            });

                            if (local == (Steps.Length - 1))
                            {
                                _scheduler.Schedule(UnlockDelayAfterLastStep, async () => await _eventPipeline.ProcessEventAsync(new GameEventUnlockRegistrationEvent(e.Type)));
                            }

                            break;
                    }
                });
            }
        }

        private class Step
        {
            public Step(TimeSpan delay, int displayTime, bool isSecond)
            {
                Delay = delay;
                DisplayTime = displayTime;
                IsSecond = isSecond;
            }

            public TimeSpan Delay { get; }
            public int DisplayTime { get; }
            public bool IsSecond { get; }
        }
    }
}