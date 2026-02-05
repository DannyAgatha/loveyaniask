using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace Plugin.Act4.Event;

public class RevivalStartProcedureEventAct4Handler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly Act4Configuration _act4Configuration;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameLanguageService _languageService;
    private readonly IExpirableLockService _lockService;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;
    private readonly ISessionManager _sessionManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;
    private readonly IBuffFactory _buffFactory;

    public RevivalStartProcedureEventAct4Handler(GameRevivalConfiguration gameRevivalConfiguration, Act4Configuration act4Configuration, IAsyncEventPipeline asyncEventPipeline,
        GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService, ISessionManager sessionManager, IReputationConfiguration reputationConfiguration,
        IRankingManager rankingManager, IExpirableLockService lockService, IGameItemInstanceFactory gameItemInstanceFactory, BattlePassQuestConfiguration battlePassQuestConfiguration,
        IBuffFactory buffFactory)
    {
        _act4Configuration = act4Configuration;
        _asyncEventPipeline = asyncEventPipeline;
        _minMaxConfiguration = minMaxConfiguration;
        _languageService = languageService;
        _sessionManager = sessionManager;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _lockService = lockService;
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _battlePassQuestConfiguration = battlePassQuestConfiguration;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IBattleEntity killer = e.Killer;
        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4Dungeon)
        {
            return;
        }
        
        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.Caligor)
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
                
                int baseExperience = session.PlayerEntity.SubClass.IsPvpSubClass() ? 200 : session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 100 : 0;
                session.AddTierExperience(baseExperience, _languageService, false);

                if (playerEntity.Family != null)
                {
                    playerEntity.Family.CurrentDayRankStat.PvpPoints++;
                }
                
                await session.EmitEventAsync(new QuestEnemyDeathEvent
                {
                    KillerSession = playerEntity.Session,
                    KilledPlayer = session.PlayerEntity
                });
                
                await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.KillXPlayerInGlacernon));
                session.PlayerEntity.Act4Dead++;

                playerEntity.Session.SendChatMessage(playerEntity.Session.GetLanguageFormat(GameDialogKey.ACT4_CHATMESSAGE_KILL_INFO, playerEntity.Act4Kill, playerEntity.Act4Dead),
                    ChatMessageColorType.Yellow);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.ACT4_CHATMESSAGE_KILL_INFO, session.PlayerEntity.Act4Kill, session.PlayerEntity.Act4Dead), ChatMessageColorType.Yellow);

                switch (session.PlayerEntity.Faction)
                {
                    case FactionType.Angel:

                        // For every demon, send "killer killed an angel"
                        _sessionManager.Broadcast(x =>
                        {
                            string factionKey = _languageService.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_PVP_ANGELS, x.UserLanguage);

                            return session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CHATMESSAGE_PVP_KILL, x.UserLanguage,
                                factionKey, playerEntity.Name), ChatMessageColorType.Green);
                        }, new FactionBroadcast(FactionType.Demon));

                        // For every angel, send "nick died from a demon"
                        _sessionManager.Broadcast(x =>
                        {
                            string factionKey = _languageService.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_PVP_DEMONS, x.UserLanguage);

                            return session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CHATMESSAGE_PVP_DEATH, x.UserLanguage,
                                factionKey, session.PlayerEntity.Name), ChatMessageColorType.Red);
                        }, new FactionBroadcast(FactionType.Angel));

                        break;
                    case FactionType.Demon:

                        _sessionManager.Broadcast(x =>
                        {
                            string factionKey = _languageService.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_PVP_ANGELS, x.UserLanguage);

                            return session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CHATMESSAGE_PVP_DEATH, x.UserLanguage,
                                factionKey, session.PlayerEntity.Name), ChatMessageColorType.Red);
                        }, new FactionBroadcast(FactionType.Demon));

                        _sessionManager.Broadcast(x =>
                        {
                            string factionKey = _languageService.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_PVP_DEMONS, x.UserLanguage);

                            return session.PlayerEntity.GenerateSayPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_CHATMESSAGE_PVP_KILL, x.UserLanguage,
                                factionKey, playerEntity.Name), ChatMessageColorType.Green);
                        }, new FactionBroadcast(FactionType.Angel));

                        break;
                }
            }
        }

        DateTime actualTime = DateTime.UtcNow;

        if (killer is null or IMonsterEntity { SummonerType: not VisualType.Player })
        {
            await Penalty(e);
            session.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.ForcedRevivalDelay, RevivalType.DontPayRevival, ForcedType.Forced);
            session.PlayerEntity.UpdateAskRevival(actualTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.BasicRevival);
            return;
        }

        session.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.Act4SealRevivalDelay, RevivalType.DontPayRevival, ForcedType.Act4SealRevival);
    }

    private async Task HandleReputation(IClientSession session, IPlayerEntity killer)
    {
        if (killer.Session.IpAddress == session.IpAddress)
        {
            return;
        }

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

    private async Task Penalty(RevivalStartProcedureEvent e)
    {
        PlayerRevivalPenalization playerRevivalPenalization = _revivalConfiguration.PlayerRevivalPenalization;
        if (e.Sender.PlayerEntity.Level <= playerRevivalPenalization.MaxLevelWithoutRevivalPenalization)
        {
            return;
        }

        int amount = e.Sender.PlayerEntity.Level < playerRevivalPenalization.MaxLevelWithDignityPenalizationIncrement
            ? e.Sender.PlayerEntity.Level * playerRevivalPenalization.DignityPenalizationIncrementMultiplier
            : playerRevivalPenalization.MaxLevelWithDignityPenalizationIncrement * playerRevivalPenalization.DignityPenalizationIncrementMultiplier;

        await e.Sender.PlayerEntity.RemoveDignity(amount, _minMaxConfiguration, _languageService, _reputationConfiguration, _rankingManager.TopReputation);
    }
}