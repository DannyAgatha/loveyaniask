using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act5;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Act5;

public class Act5OpenNpcRunEventHandler : IAsyncEventProcessor<Act5OpenNpcRunEvent>
{
    private readonly IAct5NpcRunCraftItemConfiguration _act5NpcRunCraftItemConfiguration;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;

    public Act5OpenNpcRunEventHandler(IAct5NpcRunCraftItemConfiguration act5NpcRunCraftItemConfiguration,
        IGameItemInstanceFactory gameItemInstanceFactory, IItemsManager itemsManager, IGameLanguageService gameLanguage)
    {
        _act5NpcRunCraftItemConfiguration = act5NpcRunCraftItemConfiguration;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _itemsManager = itemsManager;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(Act5OpenNpcRunEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        NpcRunType npcRunType = e.NpcRunType;
        bool isConfirm = e.IsConfirm;

        if (session?.CurrentMapInstance == null)
        {
            return;
        }

        if (!session.IsInAct5())
        {
            return;
        }

        if (!isConfirm)
        {
            session.SendQnaPacket($"n_run {(short)npcRunType} 0 0 0 1", session.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_EXCHANGE));
            return;
        }

        Act5NpcRunCraftItemConfig config = _act5NpcRunCraftItemConfiguration.GetConfigByNpcRun(npcRunType);
        if (config == null)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.ACT5_CHATMESSAGE_NO_RECIPE));
            return;
        }

        if (config.ItemByClass is true && session.PlayerEntity.Class is ClassType.Adventurer)
        {
            return;
        }

        foreach (Act5NpcRunCraftItemConfigItem neededItem in config.NeededItems)
        {
            if (session.PlayerEntity.HasItem(neededItem.Item, (short)neededItem.Amount))
            {
                continue;
            }

            string getItemName = _itemsManager.GetItem(neededItem.Item).GetItemName(_gameLanguage, session.UserLanguage);
            int missingItems = session.CountMissingItems(neededItem.Item, (short)neededItem.Amount);
            session.SendMsg(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, missingItems, getItemName), MsgMessageType.Middle);
            return;
        }

        int craftedItem = config.CraftedItem;

        if (config.ItemByClass is true)
        {
            craftedItem += (int)session.PlayerEntity.Class;
        }

        if (!session.PlayerEntity.HasSpaceFor(craftedItem, (short)config.Amount))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), ChatMessageColorType.Yellow);
            return;
        }

        foreach (Act5NpcRunCraftItemConfigItem item in config.NeededItems)
        {
            await session.RemoveItemFromInventory(item.Item, (short)item.Amount);
        }

        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(craftedItem, config.Amount);
        await session.AddNewItemToInventory(newItem, true, ChatMessageColorType.Yellow);
    }
}