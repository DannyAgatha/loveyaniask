using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Quests;

public class QuestDailyHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly INpcRunTypeQuestsConfiguration _npcRunTypeQuestsConfiguration;
    private readonly IQuestManager _questManager;
    private readonly IRandomGenerator _randomGenerator;

    public QuestDailyHandler(INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration, IRandomGenerator randomGenerator, IQuestManager questManager, IGameLanguageService langService)
    {
        _npcRunTypeQuestsConfiguration = npcRunTypeQuestsConfiguration;
        _randomGenerator = randomGenerator;
        _questManager = questManager;
        _langService = langService;
    }

    public NpcRunType[] NpcRunTypes => new[]
    {
        NpcRunType.ICY_FLOWERS_MISSION,
        NpcRunType.HEAT_POTION_MISSION,
        NpcRunType.JOHN_MISSION,
        NpcRunType.AKAMUR_MISSION,
        NpcRunType.DAILY_MISSION_TAROT
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

        List<int> possibleQuest = _npcRunTypeQuestsConfiguration.GetPossibleQuestsByNpcRunType(e.NpcRunType);
        int rnd = _randomGenerator.RandomNumber(0, possibleQuest.Count);

        QuestDto randomQuest = _questManager.GetQuestById(possibleQuest[rnd]);
        if (randomQuest == null)
        {
            return;
        }

        if (session.PlayerEntity.Level < randomQuest.MinLevel)
        {
            session.SendMsg(_langService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_LOW_LEVEL, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new AddQuestEvent(randomQuest.Id, randomQuest.IsBlue ? QuestSlotType.SECONDARY : QuestSlotType.GENERAL));
    }
}