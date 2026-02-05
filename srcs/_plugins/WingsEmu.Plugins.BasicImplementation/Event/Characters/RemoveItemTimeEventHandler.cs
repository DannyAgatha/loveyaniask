using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class RemoveItemTimeEventHandler : IAsyncEventProcessor<InventoryExpiredItemsEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    public RemoveItemTimeEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(InventoryExpiredItemsEvent e, CancellationToken cancellation)
    {
        if (!e.Sender.PlayerEntity.GetAllPlayerInventoryItems().Any())
        {
            return;
        }

        DateTime date = DateTime.UtcNow;

        foreach (InventoryItem item in e.Sender.PlayerEntity.GetAllPlayerInventoryItems())
        {
            if (item == null)
            {
                continue;
            }

            if (!item.ItemInstance.IsBound)
            {
                continue;
            }

            if (item.ItemInstance.ItemDeleteTime == null)
            {
                continue;
            }

            if (item.ItemInstance.ItemDeleteTime >= date)
            {
                continue;
            }

            string itemName = item.ItemInstance.GameItem.GetItemName(_gameLanguage, e.Sender.UserLanguage);

            e.Sender.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_TIMEOUT, e.Sender.UserLanguage, itemName), ChatMessageColorType.Red);

            await e.Sender.RemoveItemFromInventory(item: item, isEquiped: item.IsEquipped);

            if (e.Sender.ShouldSendAmuletPacket(item.ItemInstance.GameItem.EquipmentSlot))
            {
                e.Sender.SendEmptyAmuletBuffPacket();
            }
        }
    }
}