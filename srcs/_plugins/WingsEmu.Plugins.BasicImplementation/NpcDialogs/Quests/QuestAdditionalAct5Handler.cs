using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Quests;

public class QuestAdditionalAct5Handler : INpcDialogAsyncHandler
{
    // This nrun should be raised only for dialog-related quests
    private static readonly HashSet<QuestType> NpcTalkQuests = new()
    {
        QuestType.DIALOG,
        QuestType.DIALOG_2,
        QuestType.DELIVER_ITEM_TO_NPC,
        QuestType.GIVE_ITEM_TO_NPC,
        QuestType.GIVE_ITEM_TO_NPC_2,
        QuestType.GIVE_NPC_GOLD,
        QuestType.DIALOG_WHILE_WEARING,
        QuestType.DIALOG_WHILE_HAVING_ITEM,
        QuestType.WIN_RAID_AND_TALK_TO_NPC,
    };

    private readonly IQuestManager _questManager;

    public QuestAdditionalAct5Handler(IQuestManager questManager) => _questManager = questManager;

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.QUEST_ADDITIONAL_ACT5 };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        QuestNpcDto npcQuest = _questManager.GetNpcBlueAlertQuestByQuestId(e.Argument);
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);

        if (!session.PlayerEntity.HasQuestWithId(e.Argument))
        {
            return;
        }

        if (npcQuest == null || npcEntity == null)
        {
            return;
        }

        if (npcQuest.NpcVnum != npcEntity.NpcVNum)
        {
            return;
        }

        CharacterQuest characterQuest = session.PlayerEntity.GetQuestById(e.Argument);
        if (!NpcTalkQuests.Contains(characterQuest.Quest.QuestType))
        {
            return;
        }

        await session.EmitEventAsync(new QuestNpcTalkEvent(characterQuest, npcEntity, true));
    }
}