using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventoryDropItemEventHandler : IAsyncEventProcessor<InventoryDropItemEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public InventoryDropItemEventHandler(IGameLanguageService gameLanguage, GeneralServerConfiguration generalServerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _generalServerConfiguration = generalServerConfiguration;
    }

    public async Task HandleAsync(InventoryDropItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryType inventoryType = e.InventoryType;
        short slot = e.Slot;
        int amount = e.Amount;

        if (session.PlayerEntity.LastPutItem > DateTime.UtcNow)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (inventoryType != InventoryType.Equipment && inventoryType != InventoryType.Main && inventoryType != InventoryType.Etc)
        {
            return;
        }

        if (session.IsActionForbidden())
        {
            return;
        }

        InventoryItem invItem = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
        if (invItem == null)
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (invItem.ItemInstance.Amount < amount)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DROP_SHOUTMESSAGE_BAD_DROP_AMOUNT, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!invItem.ItemInstance.GameItem.IsDroppable)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_NOT_DROPPABLE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (amount <= 0)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DROP_SHOUTMESSAGE_BAD_DROP_AMOUNT, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (amount > _generalServerConfiguration.MaxItemAmount)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DROP_SHOUTMESSAGE_BAD_DROP_AMOUNT, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.CurrentMapInstance.Drops.Count > 200)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DROP_SHOUTMESSAGE_FULL, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        GameItemInstance item = invItem.ItemInstance;

        MapItem droppedItem = session.CurrentMapInstance.PutItem((ushort)amount, ref item, session);
        if (droppedItem == null)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_NOT_DROPPABLE_HERE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.RemoveItemFromInventory(amount: amount, item: invItem);
        droppedItem.BroadcastDrop();
        session.PlayerEntity.LastPutItem = DateTime.UtcNow + TimeSpan.FromMilliseconds(100);
    }
}