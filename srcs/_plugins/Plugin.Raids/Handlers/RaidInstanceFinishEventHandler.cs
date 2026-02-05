using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quests;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Configuration;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidInstanceFinishEventHandler : IAsyncEventProcessor<RaidInstanceFinishEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _languageService;
    private readonly IQuestManager _questManager;
    private readonly RaidConfiguration _raidConfiguration;
    private readonly IRaidManager _raidManager;
    private readonly ISessionManager _sessionManager;
    private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;

    public RaidInstanceFinishEventHandler(RaidConfiguration raidConfiguration, IRaidManager raidManager, IAsyncEventPipeline eventPipeline, ISessionManager sessionManager,
        IGameLanguageService languageService, IQuestManager questManager, BattlePassQuestConfiguration battlePassQuestConfiguration)
    {
        _raidConfiguration = raidConfiguration;
        _raidManager = raidManager;
        _eventPipeline = eventPipeline;
        _sessionManager = sessionManager;
        _languageService = languageService;
        _questManager = questManager;
        _battlePassQuestConfiguration = battlePassQuestConfiguration;
    }

    public async Task HandleAsync(RaidInstanceFinishEvent e, CancellationToken cancellation)
    {
        if (e.RaidParty.Finished)
        {
            return;
        }

        _raidManager.UnregisterRaidFromRaidPublishList(e.RaidParty);

        RaidWindowType windowType = RaidWindowType.MISSION_FAIL;
        DateTime currentTime = DateTime.UtcNow;
        
        if (e.RaidFinishType != RaidFinishType.Disbanded)
        {
            foreach (RaidSubInstance subInstance in e.RaidParty.Instance.RaidSubInstances.Values)
            {
                foreach (IMonsterEntity monsterEntity in subInstance.MapInstance.GetAliveMonsters())
                {
                    if (monsterEntity.IsBoss)
                    {
                        continue;
                    }

                    subInstance.MapInstance.DespawnMonster(monsterEntity);
                    subInstance.MapInstance.RemoveMonster(monsterEntity);
                }
            }
        }

        switch (e.RaidFinishType)
        {
            case RaidFinishType.Disbanded:
                await _eventPipeline.ProcessEventAsync(new RaidInstanceDestroyEvent(e.RaidParty), cancellation);
                return;
            case RaidFinishType.MissionClear:
                windowType = RaidWindowType.MISSION_CLEAR;
                if (e.RaidParty.Leader.PlayerEntity.Raid.IsMarathonMode)
                {
                    e.RaidParty.Instance.SetFinishSlowMoDate(currentTime);
                }
                else
                {
                    e.RaidParty.Instance.SetFinishSlowMoDate(currentTime + _raidConfiguration.RaidSlowMoDelay);
                }
                break;
            case RaidFinishType.TimeIsUp:
                windowType = RaidWindowType.TIMES_UP;
                break;
            case RaidFinishType.NoLivesLeft:
                windowType = RaidWindowType.NO_LIVES_LEFT;
                break;
        }

        IClientSession[] sessions = e.RaidParty.Members.ToArray();

        foreach (IClientSession session in sessions)
        {
            if (session.PlayerEntity.HasBuff(BuffVnums.ETERNAL_ICE))
            {
                await session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.ETERNAL_ICE);
            }
            
            if (!session.PlayerEntity.IsAlive())
            {
                await session.EmitEventAsync(new RevivalReviveEvent());
            }

            if (e.RaidFinishType == RaidFinishType.MissionClear)
            {
                session.TrySendRaidBossDeadPackets();
                await session.EmitEventAsync(new RaidWonEvent());
                await CheckRaidQuest(session, e.RaidParty.Type);

                await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteRaidXTime, firstData: (long)e.RaidParty.Type));
                await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXRaids));
                
                long timePassedToFinishRaid = (long)(DateTime.Now - e.RaidParty.Instance.StartDate).TotalSeconds;

                BattlePassQuest findQuest = _battlePassQuestConfiguration.Quests.FirstOrDefault(s => s.MissionType == MissionType.SuccessRaidInXTime && s.FirstData == (long)e.RaidParty.Type);
                
                if (findQuest != null && timePassedToFinishRaid <= findQuest.MaxObjectiveValue)
                {
                    await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SuccessRaidInXTime, firstData: (long)e.RaidParty.Type));
                }

                session.PlayerEntity.SkillComponent.ResetSpSkillCooldowns = DateTime.UtcNow;
                HandleTierExperience(session, e.RaidParty.Type);
            }

            session.SendRaidUiPacket(e.RaidParty.Type, windowType);
            session.SendRemoveClockPacket();
            session.SendRaidPacket(RaidPacketType.LIST_MEMBERS);
        }

        if (e.RaidFinishType == RaidFinishType.MissionClear)
        {
            IMonsterEntity raidBoss = FindBossMap(e.RaidParty);
            BroadcastRaidFinishMessage(e.RaidParty);
            await e.RaidParty.Leader.EmitEventAsync(new RaidTargetKilledEvent { DamagerCharactersIds = raidBoss.PlayersDamage.Keys.ToArray() });
            await _eventPipeline.ProcessEventAsync(new RaidGiveRewardsEvent(e.RaidParty, raidBoss, e.RaidParty.Instance.RaidReward), cancellation);
        }
        else if (e.RaidFinishType != RaidFinishType.Disbanded)
        {
            await e.RaidParty.Leader.EmitEventAsync(new RaidLostEvent());
        }
        
        TimeSpan timeElapsed  = currentTime - e.RaidParty.StartTime;
        string formattedTimeElapsed = timeElapsed.FormatElapsedTime();
        foreach (IClientSession session in sessions)
        {
            session.SendPacket(session.PlayerEntity.GenerateSayPacket(session.GetLanguageFormat(GameDialogKey.RAID_ELAPSED_TIME, e.RaidParty.Type, $"{formattedTimeElapsed}", session.UserLanguage), ChatMessageColorType.Green));
            session.SendRaidmbf();
            session.RefreshRaidMemberList(e.RaidParty.IsSpecialRaid());
            if (session.PlayerEntity?.Morph == (int)MorphType.PoisonousHamster || session.PlayerEntity?.Morph == (int)MorphType.BrownBushi)
                await session.EmitEventAsync(new GetDefaultMorphEvent());
        }

        e.RaidParty.FinishRaid(currentTime + (e.RaidParty.Leader.PlayerEntity.Raid.IsMarathonMode ? TimeSpan.FromSeconds(9) : _raidConfiguration.RaidMapDestroyDelay));
    }
    
    private void HandleTierExperience(IClientSession session, RaidType raidType)
    {
        int baseIncrement = raidType switch
        {
            RaidType.Cuby or RaidType.Ginseng or RaidType.Castra or RaidType.GiantBlackSpider => 2,
            RaidType.Slade or RaidType.ChickenKing or RaidType.Namaju or RaidType.RobberGang => 4,
            RaidType.Kertos or RaidType.Valakus or RaidType.Grenigas or RaidType.LordDraco or RaidType.Glacerus or RaidType.Laurena => 6,
            RaidType.Zenas or RaidType.Erenia or RaidType.Fernon => 8,
            _ => 0
        };
        
        int baseExperience = session.PlayerEntity.SubClass.IsPveSubClass() ? baseIncrement :
            session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? baseIncrement / 2 : 0;
        
        session.AddTierExperience(baseExperience, _languageService);
    }

    private async Task CheckRaidQuest(IClientSession session, RaidType raidType)
    {
        IEnumerable<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuestsByTypes(new[] { QuestType.WIN_RAID_AND_TALK_TO_NPC });
        foreach (CharacterQuest quest in characterQuests)
        {
            foreach (QuestObjectiveDto objective in quest.Quest.Objectives)
            {
                if (raidType != (RaidType)objective.Data0)
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = quest.ObjectiveAmount[objective.ObjectiveIndex];

                int amountLeft = questObjectiveDto.RequiredAmount - questObjectiveDto.CurrentAmount;
                if (amountLeft == 0)
                {
                    break;
                }

                questObjectiveDto.CurrentAmount++;

                if (session.PlayerEntity.IsQuestCompleted(quest))
                {
                    await session.EmitEventAsync(new QuestCompletedEvent(quest));
                }
                else
                {
                    session.RefreshQuestProgress(_questManager, quest.QuestId);
                }
                await session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                {
                    CharacterQuest = quest
                });
            }
        }
    }
    
    private void BroadcastRaidFinishMessage(RaidParty raidParty)
    {
        _sessionManager.Broadcast(x => x.GenerateMsgi2Packet(
            ChatMessageColorType.White,
            Game18NConstString.TeamCompletedRaid,
            14,
            raidParty.Leader.PlayerEntity.Name,
            FindBossMap(raidParty).MonsterVNum.ToString())
        );
    }

    private IMonsterEntity FindBossMap(RaidParty raidParty)
    {
        IMonsterEntity raidBoss = null;

        foreach (RaidSubInstance subInstance in raidParty.Instance.RaidSubInstances.Values)
        {
            foreach (IMonsterEntity monsterEntity in subInstance.DeadBossMonsters)
            {
                raidBoss = monsterEntity;
                break;
            }
        }

        return raidBoss;
    }
}