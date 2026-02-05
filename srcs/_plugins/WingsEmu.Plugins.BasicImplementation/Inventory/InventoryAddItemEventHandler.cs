using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.LegendaryDrop;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventoryAddItemEventHandler : IAsyncEventProcessor<InventoryAddItemEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;

    public InventoryAddItemEventHandler(IGameLanguageService gameLanguage, IItemsManager itemsManager)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
    }

    public async Task HandleAsync(InventoryAddItemEvent e, CancellationToken cancellation)
    {
        InventoryItem item = e.InventoryItem;
        GameItemInstance newItem = e.InventoryItem.ItemInstance;
        IClientSession session = e.Sender;

        if (newItem == null)
        {
            return;
        }

        if (newItem.ItemVNum == (short)ItemVnums.GOLD)
        {
            return;
        }

        if (!session.PlayerEntity.HasSpaceFor(newItem.ItemVNum, (short)newItem.Amount))
        {
            if (e.SendAsGiftIfFull)
            {
                await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newItem));
                return;
            }

            switch (e.MessageErrorType)
            {
                case MessageErrorType.Chat:
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), ChatMessageColorType.Yellow);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), MsgMessageType.Middle);
                    break;
                case MessageErrorType.Shop:
                    session.SendSMemo(SmemoType.Error, _gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage));
                    break;
            }

            return;
        }

        if (newItem.SerialTracker != null)
        {
            IEnumerable<InventoryItem> currentItems = session.PlayerEntity.GetAllPlayerInventoryItems();
            if (currentItems.Any(s => s?.ItemInstance.SerialTracker != null && s.ItemInstance.SerialTracker == newItem.SerialTracker))
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[DUPLICATED_ITEM] player: {session.PlayerEntity.Name} | masterAccountId: {session.Account.MasterAccountId} | hwid: {session.HardwareId} | has duplicated item => vnum: {newItem.ItemVNum} | serialTracker: {newItem.SerialTracker}");
            }
        }
        
        InventoryItem secondItem = session.PlayerEntity.FindItemWithoutFullStack(newItem.ItemVNum, (short)newItem.Amount);
        if (secondItem != null && !secondItem.ItemInstance.GameItem.IsNotStackableInventoryType() && !e.IsByMovePacket)
        {
            secondItem.ItemInstance.Amount += newItem.Amount;
            session.SendInventoryAddPacket(secondItem);
        }
        else
        {
            if (item.InventoryType == InventoryType.Equipment && item.ItemInstance.Amount != 1)
            {
                item.ItemInstance.Amount = 1;
            }

            session.PlayerEntity.AddItemToInventory(item);
            session.SendInventoryAddPacket(item);
        }

        if (!e.ShowMessage)
        {
            return;
        }
        
        session.SendSayi(ChatMessageColorType.Green, Game18NConstString.ReceivedThisItem, 2, newItem.ItemVNum.ToString(), newItem.Amount);
    }
}