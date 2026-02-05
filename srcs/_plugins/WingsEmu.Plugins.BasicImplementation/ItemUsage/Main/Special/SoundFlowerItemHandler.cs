using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SoundFlowerItemHandler : IItemUsageByVnumHandler
{
    private readonly IQuestManager _questManager;
    private readonly INpcRunTypeQuestsConfiguration _npcRunTypeQuestsConfiguration;
    private readonly IGameLanguageService _gameLanguageService;

    public SoundFlowerItemHandler(IQuestManager questManager, INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration, IGameLanguageService gameLanguageService)
    {
        _questManager = questManager;
        _npcRunTypeQuestsConfiguration = npcRunTypeQuestsConfiguration;
        _gameLanguageService = gameLanguageService;
    }

    public long[] Vnums => [(long)ItemVnums.WILD_SOUND_FLOWER];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsOnVehicle || !session.PlayerEntity.IsAlive())
        {
            return;
        }
        
        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
            return;
        }
        
        int questId = 5981;

        QuestDto quest = _questManager.GetQuestById(questId);
        if (quest == null)
        {
            Log.Debug($"[ERROR] Quest not found: {questId}");
            return;
        }
        
        if (session.HasAlreadyQuestOrQuestline(quest, _questManager, _npcRunTypeQuestsConfiguration))
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_HAVE_QUEST, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new AddQuestEvent(questId, QuestSlotType.GENERAL));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}
