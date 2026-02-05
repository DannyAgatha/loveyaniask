using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.RecurrentJob
{
    public class RainbowBattleSystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan Start = TimeSpan.FromMinutes(5);

        private static List<(TimeSpan, int, TimeType)> _times = new();
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IRainbowBattleManager _rainbowBattleManager;
        private readonly ISessionManager _sessionManager;

        public RainbowBattleSystem(IAsyncEventPipeline eventPipeline, IRainbowBattleManager rainbowBattleManager, ISessionManager sessionManager)
        {
            _eventPipeline = eventPipeline;
            _rainbowBattleManager = rainbowBattleManager;
            _sessionManager = sessionManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[RAINBOW_SYSTEM] Start Rainbow Battle system...");

            _times = _rainbowBattleManager.Warnings.ToList();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MainProcess();
                }
                catch (Exception e)
                {
                    Log.Error("[RAINBOW_SYSTEM] ", e);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task MainProcess()
        {
            DateTime dateNow = DateTime.UtcNow;
            switch (_rainbowBattleManager.IsRegistrationActive)
            {
                case true:
                    await ProcessRegistration(dateNow);
                    return;
                case false when !_rainbowBattleManager.IsActive:
                    ProcessTime(dateNow);
                    await ProcessStart(dateNow);
                    return;
            }

            if (_rainbowBattleManager.IsActive && !_rainbowBattleManager.RainbowBattleParties.Any())
            {
                _rainbowBattleManager.IsActive = false;
                _times.Clear();
                _times = _rainbowBattleManager.Warnings.ToList();
                return;
            }

            foreach (RainbowBattleParty rainbowBattleParty in _rainbowBattleManager.RainbowBattleParties)
            {
                await ProcessRainbowBattle(rainbowBattleParty, dateNow);
            }
        }

        private async Task ProcessStart(DateTime dateNow)
        {
            if (_rainbowBattleManager.RainbowBattleProcessTime is null)
            {
                return;
            }

            if (dateNow < _rainbowBattleManager.RainbowBattleProcessTime + Start)
            {
                return;
            }

            _times.Clear();
            _times = _rainbowBattleManager.Warnings.ToList();
            await _eventPipeline.ProcessEventAsync(new RainbowBattleStartRegisterEvent());
        }

        private void ProcessTime(DateTime dateNow)
        {
            if (_rainbowBattleManager.RainbowBattleProcessTime is null)
            {
                return;
            }

            if (_times.Count < 1)
            {
                return;
            }

            (TimeSpan, int, TimeType) warning = _times.OrderBy(x => x.Item1).First();

            if (dateNow < _rainbowBattleManager.RainbowBattleProcessTime + warning.Item1)
            {
                return;
            }

            _times.Remove(warning);

            GameDialogKey gameDialogKey = warning.Item3 == TimeType.SECONDS ? GameDialogKey.GAMEEVENT_SHOUTMESSAGE_PREPARATION_SECONDS : GameDialogKey.GAMEEVENT_SHOUTMESSAGE_PREPARATION_MINUTES;

            _sessionManager.Broadcast(x =>
            {
                string rainbowBattleName = x.GetLanguage(GameDialogKey.RAINBOW_BATTLE_EVENT_NAME);
                return x.GenerateMsgPacket(x.GetLanguageFormat(gameDialogKey, rainbowBattleName, warning.Item2), MsgMessageType.Middle);
            });

            _sessionManager.Broadcast(x =>
            {
                string rainbowBattleName = x.GetLanguage(GameDialogKey.RAINBOW_BATTLE_EVENT_NAME);
                return x.GenerateMsgPacket(x.GetLanguageFormat(gameDialogKey, rainbowBattleName, warning.Item2), MsgMessageType.BottomCard);
            });
        }

        private async Task ProcessRegistration(DateTime dateTime)
        {
            if (_rainbowBattleManager.RegistrationStartTime > dateTime)
            {
                return;
            }

            await _eventPipeline.ProcessEventAsync(new RainbowBattleStartProcessRegistrationEvent());
        }

        private async Task ProcessRainbowBattle(RainbowBattleParty rainbowBattleParty, DateTime dateTime)
        {
            ProcessStartGame(rainbowBattleParty, dateTime);
            await ProcessFrozenMembers(rainbowBattleParty);
            await TryEndRainbowBattle(rainbowBattleParty, dateTime);
            await TryDestroyRainbowBattle(rainbowBattleParty, dateTime);
            await ProcessTeamPoints(rainbowBattleParty, dateTime);
            await ProcessActivityTeamPoints(rainbowBattleParty, dateTime);
            await ProcessMembersLife(rainbowBattleParty, dateTime);
        }

        private async Task ProcessActivityTeamPoints(RainbowBattleParty rainbowBattleParty, DateTime dateTime)
        {
            if (!rainbowBattleParty.Started || rainbowBattleParty.FinishTime != null)
            {
                return;
            }

            if (rainbowBattleParty.LastActivityPointsTeamAdd > dateTime)
            {
                return;
            }

            rainbowBattleParty.LastActivityPointsTeamAdd = dateTime.AddSeconds(59);
            await _eventPipeline.ProcessEventAsync(new RainbowBattleProcessActivityPointsEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }

        private async Task ProcessFrozenMembers(RainbowBattleParty rainbowBattleParty)
        {
            if (!rainbowBattleParty.Started)
            {
                return;
            }

            await _eventPipeline.ProcessEventAsync(new RainbowBattleUnfreezeProcessEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }

        private async Task ProcessTeamPoints(RainbowBattleParty rainbowBattleParty, DateTime dateTime)
        {
            if (!rainbowBattleParty.Started || rainbowBattleParty.FinishTime != null)
            {
                return;
            }

            if (rainbowBattleParty.LastPointsTeamAdd > dateTime)
            {
                return;
            }

            rainbowBattleParty.LastPointsTeamAdd = dateTime.AddSeconds(120);
            await _eventPipeline.ProcessEventAsync(new RainbowBattleProcessFlagPointsEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }

        private async Task TryEndRainbowBattle(RainbowBattleParty rainbowBattleParty, DateTime dateTime)
        {
            if (!rainbowBattleParty.Started || rainbowBattleParty.EndTime > dateTime || rainbowBattleParty.FinishTime != null)
            {
                return;
            }

            await _eventPipeline.ProcessEventAsync(new RainbowBattleEndEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }

        private async Task TryDestroyRainbowBattle(RainbowBattleParty rainbowBattleParty, DateTime dateTime)
        {
            if (rainbowBattleParty.MapInstance != null && rainbowBattleParty.MapInstance.Sessions.Count < 1)
            {
                Log.Warn("[RAINBOW_SYSTEM] Destroying Rainbow Battle instance");
                await _eventPipeline.ProcessEventAsync(new RainbowBattleDestroyEvent
                {
                    RainbowBattleParty = rainbowBattleParty
                });
                return;
            }

            if (!rainbowBattleParty.Started || rainbowBattleParty.FinishTime == null)
            {
                return;
            }

            if (rainbowBattleParty.FinishTime > dateTime)
            {
                return;
            }

            Log.Warn("[RAINBOW_SYSTEM] Destroying Rainbow Battle instance");
            await _eventPipeline.ProcessEventAsync(new RainbowBattleDestroyEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }

        private async Task ProcessMembersLife(RainbowBattleParty rainbowBattleParty, DateTime dateTime)
        {
            if (!rainbowBattleParty.Started || rainbowBattleParty.FinishTime != null)
            {
                return;
            }

            if (rainbowBattleParty.LastMembersLife > dateTime)
            {
                return;
            }

            rainbowBattleParty.LastMembersLife = dateTime.AddSeconds(2);
            await _eventPipeline.ProcessEventAsync(new RainbowBattleProcessLifeEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }

        private void ProcessStartGame(RainbowBattleParty rainbowBattleParty, in DateTime time)
        {
            if (rainbowBattleParty.Started)
            {
                return;
            }

            if (rainbowBattleParty.StartTime.AddSeconds(5) > time)
            {
                return;
            }

            rainbowBattleParty.Started = true;

            foreach (IClientSession session in rainbowBattleParty.MapInstance.Sessions)
            {
                session.SendCondPacket();
            }

            rainbowBattleParty.MapInstance.Broadcast(x => x.GenerateMsgPacket(x.GetLanguage(GameDialogKey.RAINBOW_BATTLE_SHOUTMESSAGE_START), MsgMessageType.Middle));
        }
    }
}