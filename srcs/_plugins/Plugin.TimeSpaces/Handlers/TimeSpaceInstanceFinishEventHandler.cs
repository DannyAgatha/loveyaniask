using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.DbServer.TimeSpaceService;
using WingsAPI.Data.TimeSpace;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceInstanceFinishEventHandler : IAsyncEventProcessor<TimeSpaceInstanceFinishEvent>
{
    private static readonly HashSet<QuestType> TimeSpaceCompletionQuestTypes = new() { QuestType.COMPLETE_TIMESPACE, QuestType.COMPLETE_TIMESPACE_WITH_ATLEAST_X_POINTS };
    private readonly IGameLanguageService _gameLanguageService;
    private readonly ISubActConfiguration _subActConfiguration;
    private readonly ITimeSpaceConfiguration _timeSpaceConfiguration;
    private readonly ITimeSpaceService _timeSpaceService;

    public TimeSpaceInstanceFinishEventHandler(IGameLanguageService gameLanguageService, ITimeSpaceService timeSpaceService, ISubActConfiguration subActConfiguration,
        ITimeSpaceConfiguration timeSpaceConfiguration)
    {
        _gameLanguageService = gameLanguageService;
        _timeSpaceService = timeSpaceService;
        _subActConfiguration = subActConfiguration;
        _timeSpaceConfiguration = timeSpaceConfiguration;
    }

    public async Task HandleAsync(TimeSpaceInstanceFinishEvent e, CancellationToken cancellation)
    {
        TimeSpaceParty timeSpaceParty = e.TimeSpaceParty;
        long? victimId = e.VictimId;

        if (timeSpaceParty == null || timeSpaceParty.Finished)
        {
            return;
        }

        switch (e.TimeSpaceFinishType)
        {
            case TimeSpaceFinishType.SUCCESS:
                await ProcessSuccessTimeSpace(timeSpaceParty);
                break;
            default:
                ProcessFailTimeSpace(timeSpaceParty, e.TimeSpaceFinishType, victimId);
                break;
        }

        foreach (TimeSpaceSubInstance subInstance in timeSpaceParty.Instance.TimeSpaceSubInstances.Values)
        {
            subInstance.MapInstance.AIDisabled = true;
            subInstance.TimeSpaceWaves.Clear();
            foreach (IMonsterEntity monsterEntity in subInstance.MapInstance.GetAliveMonsters())
            {
                subInstance.MapInstance.DespawnMonster(monsterEntity);
                subInstance.MapInstance.RemoveMonster(monsterEntity);
            }
        }

        timeSpaceParty.Destroy = true;
        timeSpaceParty.FinishTimeSpace(DateTime.UtcNow.AddMinutes(1));
    }

    private void ProcessFailTimeSpace(TimeSpaceParty timeSpaceParty, TimeSpaceFinishType timeSpaceFinishType, long? victimId = null)
    {
        foreach (IClientSession member in timeSpaceParty.Members)
        {
            member.SendRemoveRedClockPacket();

            if (victimId.HasValue && member.PlayerEntity.Id != victimId.Value && timeSpaceFinishType == TimeSpaceFinishType.OUT_OF_LIVES)
            {
                member.SendScorePacket(TimeSpaceFinishType.TEAM_MEMBER_OUT_OF_LIVES);
                continue;
            }

            member.SendScorePacket(timeSpaceFinishType);
        }
    }

    private async Task ProcessSuccessTimeSpace(TimeSpaceParty timeSpaceParty)
    {
        CalculatePenalty(timeSpaceParty);
        bool isNewRecord = false;

        if (timeSpaceParty.IsChallengeMode)
        {
            TimeSpaceIsNewRecordResponse newRecord = await _timeSpaceService.IsNewRecord(new TimeSpaceIsNewRecordRequest
            {
                TimeSpaceId = timeSpaceParty.TimeSpaceId,
                Record = timeSpaceParty.Instance.Score
            });
            isNewRecord = newRecord.IsNewRecord;
        }

        bool perfectMonsters = true;

        foreach (TimeSpaceSubInstance map in timeSpaceParty.Instance.TimeSpaceSubInstances.Values)
        {
            if (!map.MapInstance.GetAliveMonsters(m => m.SummonerType is not VisualType.Player).Any())
            {
                continue;
            }

            perfectMonsters = false;
            break;
        }

        bool perfectMaps = timeSpaceParty.Instance.TimeSpaceSubInstances.Values.Where(map
            => map.MapInstance.Id != timeSpaceParty.Instance.SpawnInstance.MapInstance.Id).All(map
            => timeSpaceParty.Instance.VisitedRooms.Contains(map.MapInstance.Id));

        bool perfectProtectedNpcs = timeSpaceParty.Instance.KilledProtectedNpcs == 0;
        timeSpaceParty.Instance.SavedNpcs = timeSpaceParty.Instance.ProtectedNpcs.Count - timeSpaceParty.Instance.KilledProtectedNpcs;

        if (timeSpaceParty.Instance.SavedNpcs < 0)
        {
            timeSpaceParty.Instance.SavedNpcs = 0;
        }

        TimeSpaceFinishType type = TimeSpaceFinishType.SUCCESS;
        if (isNewRecord)
        {
            type = TimeSpaceFinishType.SUCCESS_HIGH_SCORE;
            await _timeSpaceService.SetNewRecord(new TimeSpaceNewRecordRequest
            {
                TimeSpaceRecordDto = new TimeSpaceRecordDto
                {
                    TimeSpaceId = timeSpaceParty.TimeSpaceId,
                    CharacterName = timeSpaceParty.Leader.PlayerEntity.Name,
                    Date = DateTime.UtcNow,
                    Record = timeSpaceParty.Instance.Score
                }
            });
        }

        int goldReward = timeSpaceParty.CalculateGoldReward();
        long generateExp = timeSpaceParty.CalculateExperience();
        foreach (IClientSession member in timeSpaceParty.Members)
        {
            member.SendRemoveRedClockPacket();

            double penalty = member.GetTimeSpacePenalty();
            if (penalty != 0)
            {
                member.SendChatMessage(member.GetLanguageFormat(GameDialogKey.TIMESPACE_CHATMESSAGE_TIME_SPACE_PENALTY, penalty), ChatMessageColorType.Yellow);
            }

            if (timeSpaceParty.ItemVnumToRemove.HasValue)
            {
                InventoryItem itemToRemove = member.PlayerEntity.GetFirstItemByVnum(timeSpaceParty.ItemVnumToRemove.Value);
                if (itemToRemove != null)
                {
                    await member.RemoveItemFromInventory(item: itemToRemove);
                }
            }

            member.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_WAIT_REWARD, member.UserLanguage), MsgMessageType.Middle);
            goldReward = (int)(goldReward * (1 - penalty / 100.0));
            if (goldReward > 0 && !timeSpaceParty.IsEasyMode)
            {
                await member.EmitEventAsync(new GenerateGoldEvent(goldReward));
            }

            if (generateExp != 0 && !timeSpaceParty.IsEasyMode)
            {
                await member.EmitEventAsync(new AddExpEvent(generateExp, LevelType.Level));
            }

            if (timeSpaceParty.TimeSpaceInformation.IsHidden || timeSpaceParty.TimeSpaceInformation.IsSpecial)
            {
                await member.EmitEventAsync(new GenerateReputationEvent
                {
                    Amount = (timeSpaceParty.TimeSpaceInformation.ReputationMultiplier ?? 30) * timeSpaceParty.TimeSpaceInformation.MinLevel,
                    SendMessage = true
                });

                if (!member.PlayerEntity.CompletedTimeSpaces.Contains(timeSpaceParty.TimeSpaceId))
                {
                    member.PlayerEntity.CompletedTimeSpaces.Add(timeSpaceParty.TimeSpaceId);
                    timeSpaceParty.FirstCompletedTimeSpaceIds.Add(member.PlayerEntity.Id);
                }
            }
            else if (!member.PlayerEntity.CompletedTimeSpaces.Contains(timeSpaceParty.TimeSpaceId))
            {
                timeSpaceParty.FirstCompletedTimeSpaceIds.Add(member.PlayerEntity.Id);
                member.PlayerEntity.CompletedTimeSpaces.Add(timeSpaceParty.TimeSpaceId);
                await member.EmitEventAsync(new GenerateReputationEvent
                {
                    Amount = (timeSpaceParty.TimeSpaceInformation.ReputationMultiplier ?? 30) * timeSpaceParty.TimeSpaceInformation.MinLevel,
                    SendMessage = true
                });

                member.SendRsfiPacket(_subActConfiguration, _timeSpaceConfiguration);
            }

            member.SendScorePacket(type, perfectMonsters, perfectProtectedNpcs, perfectMaps);
            member.SendRsfmPacket(TimeSpaceAction.TIMESPACE_COMPLETE);

            HandleTsQuests(member, timeSpaceParty.TimeSpaceId, timeSpaceParty.IsChallengeMode, timeSpaceParty.Instance.Score);
        }
    }

    private void CalculatePenalty(TimeSpaceParty timeSpaceParty)
    {
        double penalty = timeSpaceParty.GetTimeSpaceScorePenalty() / 100.0;

        if (penalty <= 0)
        {
            return;
        }

        int finalScore = (int)(timeSpaceParty.Instance.Score * (1 - penalty));
        timeSpaceParty.Instance.UpdateFinalScore(finalScore);
    }

    private void HandleTsQuests(IClientSession member, long timeSpaceId, bool isChallengeMode, int score)
    {
        IEnumerable<CharacterQuest> tsQuests = member.PlayerEntity.GetCurrentQuestsByTypes(TimeSpaceCompletionQuestTypes).ToArray();
        if (!tsQuests.Any())
        {
            return;
        }

        foreach (CharacterQuest characterQuest in tsQuests)
        {
            foreach (QuestObjectiveDto objective in characterQuest.Quest.Objectives)
            {
                if (objective.Data0 != timeSpaceId)
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                switch (characterQuest.Quest.QuestType)
                {
                    case QuestType.COMPLETE_TIMESPACE:

                        if (questObjectiveDto.CurrentAmount < questObjectiveDto.RequiredAmount)
                        {
                            questObjectiveDto.CurrentAmount++;
                        }

                        member.EmitEventAsync(new QuestCompletedEvent(characterQuest));
                        break;

                    case QuestType.COMPLETE_TIMESPACE_WITH_ATLEAST_X_POINTS:
                        if (!isChallengeMode)
                        {
                            break;
                        }

                        questObjectiveDto.CurrentAmount = score;

                        member.EmitEventAsync(new QuestCompletedEvent(characterQuest));
                        break;
                }
            }
        }
    }
}