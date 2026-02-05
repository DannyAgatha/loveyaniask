using PhoenixLib.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Items;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace Plugin.Act4.Event
{
    public class Act4CaligorRevivalStartProcedureEventHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
    {
        private readonly Act4Configuration _act4Configuration;
        private readonly IGameLanguageService _languageService;
        private readonly GameMinMaxConfiguration _minMaxConfiguration;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly PlayerRevivalConfiguration _revivalConfiguration;
        private readonly ISessionManager _sessionManager;
        private readonly IExpirableLockService _lockService;
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IBuffFactory _buffFactory;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;

        public Act4CaligorRevivalStartProcedureEventHandler(GameRevivalConfiguration gameRevivalConfiguration,
            GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService, ISessionManager sessionManager, IReputationConfiguration reputationConfiguration,
            IRankingManager rankingManager, Act4Configuration act4Configuration, IExpirableLockService lockService, IAsyncEventPipeline asyncEventPipeline, IBuffFactory buffFactory,
            IGameItemInstanceFactory gameItemInstanceFactory)
        {
            _minMaxConfiguration = minMaxConfiguration;
            _languageService = languageService;
            _sessionManager = sessionManager;
            _reputationConfiguration = reputationConfiguration;
            _rankingManager = rankingManager;
            _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
            _act4Configuration = act4Configuration;
            _lockService = lockService;
            _asyncEventPipeline = asyncEventPipeline;
            _buffFactory = buffFactory;
            _gameItemInstanceFactory = gameItemInstanceFactory;
        }

        public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IBattleEntity killer = e.Killer;
            if (session.PlayerEntity.IsAlive())
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.Caligor)
            {
                return;
            }

            if (session.PlayerEntity.IsOnVehicle)
            {
                await session.EmitEventAsync(new RemoveVehicleEvent());
            }

            await session.PlayerEntity.RemoveBuffsOnDeathAsync();
            session.RefreshStat();

            if (killer?.Faction != session.PlayerEntity.Faction)
            {
                IPlayerEntity playerEntity = killer switch
                {
                    IPlayerEntity player => player,
                    IMateEntity mateEntity => mateEntity.Owner,
                    IMonsterEntity monsterEntity => monsterEntity.SummonerType is VisualType.Player && monsterEntity.SummonerId.HasValue
                        ? monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)
                        : null,
                    _ => null
                };

                if (playerEntity != null)
                {
                    if (!session.PlayerEntity.IsGettingLosingReputation)
                    {
                        if (session.PlayerEntity.DeathsOnAct4 < 10)
                        {
                            session.PlayerEntity.DeathsOnAct4++;
                        }
                        else
                        {
                            session.PlayerEntity.IsGettingLosingReputation = true;
                            await _lockService.TryAddTemporaryLockAsync($"game:locks:character:{session.PlayerEntity.Id}:act-4-less-rep", DateTime.UtcNow.Date.AddDays(1));
                            session.SendChatMessage(session.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_LESS_REPUTATION), ChatMessageColorType.Red);
                        }
                    }
                    else
                    {
                        session.SendChatMessage(session.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_LESS_REPUTATION), ChatMessageColorType.Red);
                    }

                    await HandleReputation(session, playerEntity);
                    await playerEntity.Session.EmitEventAsync(new Act4KillEvent { TargetId = session.PlayerEntity.Id });
                    playerEntity.Act4Kill++;

                    if (playerEntity.Family != null)
                    {
                        playerEntity.Family.CurrentDayRankStat.PvpPoints++;
                    }

                    await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.KillXPlayerInGlacernon));
                    session.PlayerEntity.Act4Dead++;
                
                    switch (session.PlayerEntity.Faction)
                    {
                        case FactionType.Angel:
                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_KILLER, x.UserLanguage,
                                playerEntity.Name), ChatMessageColorType.Green), new FactionBroadcast(FactionType.Demon));

                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_DEATH, x.UserLanguage,
                                session.PlayerEntity.Name), ChatMessageColorType.Red), new FactionBroadcast(FactionType.Angel));

                            break;

                        case FactionType.Demon:

                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_DEATH, x.UserLanguage,
                                session.PlayerEntity.Name), ChatMessageColorType.Red), new FactionBroadcast(FactionType.Demon));

                            _sessionManager.Broadcast(x => session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_PVP_KILLER, x.UserLanguage,
                                playerEntity.Name), ChatMessageColorType.Green), new FactionBroadcast(FactionType.Angel));

                            break;
                    }
                }
            }
            
            DateTime currentTime = DateTime.UtcNow;
            e.Sender.PlayerEntity.UpdateRevival(currentTime + _revivalConfiguration.Act4CaligorRevivalDelay, RevivalType.DontPayRevival, ForcedType.Reconnect);
            e.Sender.PlayerEntity.UpdateAskRevival(currentTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.CaligorRevival);
        }

        private async Task HandleReputation(IClientSession session, IPlayerEntity killer)
        {
            if (_act4Configuration.PvpFactionPoints)
            {
                await _asyncEventPipeline.ProcessEventAsync(new Act4FactionPointsIncreaseEvent(killer.Faction, _act4Configuration.FactionPointsPerPvpKill));
            }

            int killerReputDegree = (int)killer.GetReputationIcon(_reputationConfiguration, _rankingManager.TopReputation);
            int victimReputDegree = (int)session.PlayerEntity.GetReputationIcon(_reputationConfiguration, _rankingManager.TopReputation);
            int formulaResult = victimReputDegree * session.PlayerEntity.Level * 10 / killerReputDegree + killerReputDegree * 50 / 3;
            int finalReputation = 9 < killerReputDegree - victimReputDegree ? Convert.ToInt32(formulaResult * 0.1) : formulaResult;

            if (session.PlayerEntity.IsGettingLosingReputation || killer.IsGettingLosingReputation)
            {
                finalReputation = (int)(finalReputation * 0.05);
            }

            if (killer.IsInGroup())
            {
                foreach (IPlayerEntity member in killer.GetGroup().Members)
                {
                    if (member == null)
                    {
                        continue;
                    }

                    if (killer.MapInstance.Id != member.MapInstance?.Id)
                    {
                        continue;
                    }

                    if (member.Id != killer.Id)
                    {
                        int reputationForMember = (int)(finalReputation * 0.1);

                        if (finalReputation <= 0)
                        {
                            continue;
                        }

                        await member.Session.EmitEventAsync(new GenerateReputationEvent
                        {
                            Amount = reputationForMember,
                            SendMessage = true
                        });
                        continue;
                    }

                    if (finalReputation <= 0)
                    {
                        continue;
                    }

                    await member.Session.EmitEventAsync(new GenerateReputationEvent
                    {
                        Amount = finalReputation,
                        SendMessage = true
                    });
                }
            }
            else
            {
                await killer.Session.EmitEventAsync(new GenerateReputationEvent
                {
                    Amount = finalReputation,
                    SendMessage = true
                });
            }

            int toRemove = killerReputDegree * killer.Level * 10 / victimReputDegree + victimReputDegree * 50 / 35;

            if (session.PlayerEntity.IsGettingLosingReputation || killer.IsGettingLosingReputation)
            {
                toRemove = (int)(toRemove * 0.05);
            }

            int decrease = session.PlayerEntity.BCardComponent
                .GetAllBCardsInformation(BCardType.ChangingPlace, (byte)AdditionalTypes.ChangingPlace.DecreaseReputationLostAfterDeath, session.PlayerEntity.Level).firstData;
            toRemove = (int)(toRemove * (1 - decrease * 0.01));

            if (toRemove <= 0)
            {
                return;
            }

            await session.EmitEventAsync(new GenerateReputationEvent
            {
                Amount = -toRemove,
                SendMessage = true
            });
        }
    }
}