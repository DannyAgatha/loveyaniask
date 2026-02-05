using System.Collections.Generic;
using System.Text;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Game.Extensions.ItemExtension
{
    public static class BuyExtension
    {
        public static string GenerateSellList(this IClientSession session, long gold, short slot, int amount, int sellAmount) => $"sell_list {gold} {slot}.{amount}.{sellAmount}";

        public static void SendSellList(this IClientSession session, long gold, short slot, int amount, int sellAmount) =>
            session.SendPacket(session.GenerateSellList(gold, slot, amount, sellAmount));

        public static void SendShopContent(this IClientSession receiverSession, IPlayerEntity owner, IEnumerable<ShopPlayerItem> items)
        {
            var packetToSend = new StringBuilder($"n_inv 1 {owner.Id} 0 0");

            foreach (ShopPlayerItem item in items)
            {
                packetToSend.Append(item.GenerateShopContentSubPacket(owner));
            }

            packetToSend.Append(
                " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1");

            receiverSession.SendPacket(packetToSend.ToString());
        }

        private static string GenerateShopContentSubPacket(this ShopPlayerItem shopPlayerItem, IPlayerEntity owner)
        {
            if (shopPlayerItem == null)
            {
                return " -1";
            }

            InventoryItem inventoryItem = owner.GetItemBySlotAndType(shopPlayerItem.InventorySlot, shopPlayerItem.InventoryType);

            if (inventoryItem?.ItemInstance is null)
            {
                return " -1";
            }

            if (inventoryItem.InventoryType == InventoryType.Equipment)
            {
                return $" {((byte)inventoryItem.InventoryType).ToString()}.{shopPlayerItem.ShopSlot.ToString()}.{inventoryItem.ItemInstance.ItemVNum.ToString()}" +
                    $".{inventoryItem.ItemInstance.Rarity.ToString()}.{inventoryItem.ItemInstance.Upgrade.ToString()}.{shopPlayerItem.PricePerUnit.ToString()}";
            }

            return $" {((byte)inventoryItem.InventoryType).ToString()}.{shopPlayerItem.ShopSlot.ToString()}.{inventoryItem.ItemInstance.ItemVNum.ToString()}" +
                $".{shopPlayerItem.SellAmount.ToString()}.{shopPlayerItem.PricePerUnit.ToString()}.-1.-1.-1";
        }
    }
}