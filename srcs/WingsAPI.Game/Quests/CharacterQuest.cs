using System.Collections.Generic;
using WingsEmu.DTOs.Quests;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Quests;

public class CharacterQuest : CharacterQuestDto
{
    private readonly IRandomGenerator _randomGenerator;

    public CharacterQuest(QuestDto quest, QuestSlotType slotType, IRandomGenerator randomGenerator)
    {
        Quest = quest;
        _randomGenerator = randomGenerator;
        SlotType = slotType;
        QuestId = quest.Id;
        ObjectiveAmount = new Dictionary<int, CharacterQuestObjectiveDto>();

        InitializeObjectiveAmount();
    }

    public QuestDto Quest { get; }

    private void InitializeObjectiveAmount()
    {
        foreach (QuestObjectiveDto objective in Quest.Objectives)
        {
            ObjectiveAmount.TryAdd(objective.ObjectiveIndex, new CharacterQuestObjectiveDto
            {
                CurrentAmount = 0,
                RequiredAmount = RandomizeObjectiveAmount(objective)
            });
        }
    }

    public void ResetQuestProgress()
    {
        foreach (KeyValuePair<int, CharacterQuestObjectiveDto> objective in ObjectiveAmount)
        {
            ObjectiveAmount[objective.Key].CurrentAmount = 0;
        }
    }

    private int RandomizeObjectiveAmount(QuestObjectiveDto objective)
    {
        switch (Quest.QuestType)
        {
            // Random values
            case QuestType.KILL_MONSTER_BY_VNUM: // 1
            case QuestType.DROP_HARDCODED: // 2
            case QuestType.COMPLETE_TIMESPACE: // 7
            case QuestType.CRAFT_WITHOUT_KEEPING: // 8
            case QuestType.KILL_PLAYER_IN_REGION: // 27
                return objective.Data1 != -1 && objective.Data2 != -1 ? _randomGenerator.RandomNumber(objective.Data1, objective.Data2 + 1) :
                    objective.Data1 == -1 ? objective.Data2 : objective.Data1;
            case QuestType.DELIVER_ITEM_TO_NPC: // 4
            case QuestType.CAPTURE_WITHOUT_KEEPING: // 5
            case QuestType.CAPTURE_AND_KEEP: // 6
                return objective.Data2 != -1 && objective.Data3 != -1 ? _randomGenerator.RandomNumber(objective.Data2, objective.Data3 + 1) :
                    objective.Data2 == -1 ? objective.Data3 : objective.Data2;

            // Fixed values
            case QuestType.DROP_CHANCE: // 3
            case QuestType.DROP_IN_TIMESPACE: // 13
            case QuestType.GIVE_ITEM_TO_NPC: // 14
            case QuestType.DIALOG_WHILE_HAVING_ITEM: //16
            case QuestType.DROP_CHANCE_2: // 17
            case QuestType.COLLECT: // 20
            case QuestType.GIVE_ITEM_TO_NPC_2: // 24
                return objective.Data2;

            case QuestType.COMPLETE_TIMESPACE_WITH_ATLEAST_X_POINTS: // 11
                return objective.Data3;

            case QuestType.GIVE_NPC_GOLD: //18
                return objective.Data1 * 10000;

            // Handled differently
            case QuestType.DIALOG: // 12
            case QuestType.DIALOG_WHILE_WEARING: // 15
            case QuestType.GO_TO_MAP: // 19
            case QuestType.USE_ITEM_ON_TARGET: // 21
            case QuestType.DIALOG_2: // 22
            case QuestType.NOTHING: // 23
                break;

            case QuestType.WIN_RAID_AND_TALK_TO_NPC: // 25
                return objective.Data1;


            case QuestType.KILL_X_MOBS_SOUND_FLOWER: // 26
                return objective.Data0;

            // Not used
            case QuestType.DIE_X_TIMES: // 9
            case QuestType.EARN_REPUTATION: // 10
                return 1;
        }

        return 0;
    }
}