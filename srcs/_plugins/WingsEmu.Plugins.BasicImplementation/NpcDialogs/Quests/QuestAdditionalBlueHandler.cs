using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Quests;

public class QuestAdditionalBlueHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly IQuestManager _questManager;

    public QuestAdditionalBlueHandler(IQuestManager questManager, IGameLanguageService langService)
    {
        _questManager = questManager;
        _langService = langService;
    }

    public NpcRunType[] NpcRunTypes => new[] 
    { 
        NpcRunType.QUEST_RECEIVE_ADDITIONAL_BLUE,
    };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        QuestDto quest = _questManager.GetQuestById(e.Argument);
        if (quest == null)
        {
            return;
        }

        if (!quest.IsBlue)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"[N_RUN] Tried to add non-blue quest as a blue quest! QuestId: {e.Argument}");
            return;
        }

        if (quest.RequiredQuestId != -1 && !session.PlayerEntity.HasCompletedQuest(quest.RequiredQuestId))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"[N_RUN] Tried to add blue quest that has a previous quest! QuestId: {e.Argument}");
            return;
        }

        if (session.PlayerEntity.Level < quest.MinLevel)
        {
            session.SendMsg(_langService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_LOW_LEVEL, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new AddQuestEvent(e.Argument, QuestSlotType.SECONDARY));
    }
}