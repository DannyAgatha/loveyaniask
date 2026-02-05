using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsAPI.Game.Extensions.ItemExtension.Inventory
{
    public static class InventoryExtension
    {
        public static int CountMissingItems(this IClientSession session, int itemVNum, int amount) => amount - session.PlayerEntity.CountItemWithVnum(itemVNum);

        public static bool HasEnoughGold(this IClientSession session, long gold) => session.PlayerEntity.Gold >= gold;

        public static async Task<InventoryItem> AddNewItemToInventory(this IClientSession session, GameItemInstance itemInstance, bool showMessage = false,
            ChatMessageColorType colorType = ChatMessageColorType.Green, bool sendGiftIsFull = false, MessageErrorType errorType = MessageErrorType.Chat, short? slot = null,
            InventoryType? type = null, bool isByMovePacket = false)
        {
            var inventoryItem = new InventoryItem
            {
                InventoryType = type ?? itemInstance.GameItem.Type,
                IsEquipped = false,
                ItemInstance = itemInstance,
                CharacterId = session.PlayerEntity.Id,
                Slot = slot ?? session.PlayerEntity.GetNextInventorySlot(type ?? itemInstance.GameItem.Type)
            };
            await session.EmitEventAsync(new InventoryAddItemEvent(inventoryItem, showMessage, colorType, sendGiftIsFull, errorType, slot, type, isByMovePacket));
            return inventoryItem;
        }

        public static async Task RemoveItemFromInventory(this IClientSession session, int itemVnum = 1, int amount = 1, bool isEquiped = false, InventoryItem item = null, bool sendPackets = true)
            => await session.EmitEventAsync(new InventoryRemoveItemEvent(itemVnum, amount, isEquiped, item, sendPackets));
    }
}