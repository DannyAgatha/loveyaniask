using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Quests;

public class QuestAdditionalHandler : INpcDialogAsyncHandler
{
    private readonly GeneralQuestsConfiguration _generalQuestsConfiguration;
    private readonly IGameLanguageService _langService;
    private readonly IQuestManager _questManager;

    public QuestAdditionalHandler(IGameLanguageService langService, IQuestManager questManager, GeneralQuestsConfiguration generalQuestsConfiguration)
    {
        _langService = langService;
        _questManager = questManager;
        _generalQuestsConfiguration = generalQuestsConfiguration;
    }

    public NpcRunType[] NpcRunTypes => new[] 
    { 
        NpcRunType.QUEST_ADDITIONAL,
        NpcRunType.WINTER_MISSION_VIKINGS,
        NpcRunType.WINTER_MISSION_REINDEER,
        NpcRunType.WINTER_MISSION_WANDERER_1,
        NpcRunType.WINTER_MISSION_WANDERER_3,
        NpcRunType.TRAINER_MISSION_SP
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

        if (!_generalQuestsConfiguration.GeneralQuests.Contains(quest.Id))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"[N_RUN] Tried to add a quest as a general quest, but it's not in the configuration! QuestId: {e.Argument}");
            return;
        }

        if (session.PlayerEntity.Level < quest.MinLevel)
        {
            session.SendMsg(_langService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_LOW_LEVEL, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new AddQuestEvent(e.Argument, QuestSlotType.GENERAL));
    }
}