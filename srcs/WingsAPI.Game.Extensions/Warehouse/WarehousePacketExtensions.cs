using System.Collections.Generic;
using System.Text;
using WingsAPI.Data.Account;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Game.Extensions.Warehouse
{
    public static class WarehousePacketExtensions
    {
        #region Warehouse

        public static string GenerateStashAll(this IClientSession session, IItemsManager itemsManager, int warehouseSize, IEnumerable<AccountWarehouseItemDto> items)
        {
            var packet = new StringBuilder($"stash_all {warehouseSize.ToString()}");
            foreach (AccountWarehouseItemDto item in items)
            {
                packet.Append(' ');
                packet.Append(item.GenerateStashPacketContent(itemsManager));
            }

            return packet.ToString();
        }

        public static void SendStashDynamicItemUpdate(this IClientSession session, IItemsManager itemsManager, AccountWarehouseItemDto item, int slot)
        {
            if (item == null)
            {
                session.SendWarehouseRemovePacket(slot);
            }
            else
            {
                session.SendStashPacket(itemsManager, item);
            }
        }

        public static string GenerateStashPacketContent(this AccountWarehouseItemDto warehouseItem, IItemsManager itemsManager) =>
            GenerateStashSubPacket(itemsManager, warehouseItem.ItemInstance, warehouseItem.Slot);

        public static void SendWarehouseStashAll(this IClientSession session, IItemsManager itemsManager, int warehouseSize, IEnumerable<AccountWarehouseItemDto> items)
        {
            session.SendPacket(session.GenerateStashAll(itemsManager, warehouseSize, items));
        }

        public static string GenerateEmptyStashPacket(this IClientSession session, int slot) => $"stash {slot.ToString()}.0.0.0.0";

        public static void SendWarehouseRemovePacket(this IClientSession session, int slot) => session.SendPacket(session.GenerateEmptyStashPacket(slot));

        public static string GenerateStashPacket(this AccountWarehouseItemDto item, IItemsManager itemsManager) => $"stash {item.GenerateStashPacketContent(itemsManager)}";

        public static void SendStashPacket(this IClientSession session, IItemsManager itemsManager, AccountWarehouseItemDto item) => session.SendPacket(item.GenerateStashPacket(itemsManager));

        public static string GenerateStashSubPacket(IItemsManager itemsManager, ItemInstanceDTO itemInstanceDto, short slot)
        {
            IGameItem gameItem = itemsManager.GetItem(itemInstanceDto.ItemVNum);

            return GenerateStashSubPacket(itemInstanceDto, gameItem.Type, slot);
        }

        public static string GenerateStashSubPacket(ItemInstanceDTO itemInstanceDto, InventoryType inventoryType, short slot)
        {
            var packet = new StringBuilder();

            packet.AppendFormat("{0}.{1}.{2}.", slot.ToString(), itemInstanceDto.ItemVNum.ToString(), ((byte)inventoryType).ToString());
            switch (inventoryType)
            {
                case InventoryType.Equipment:
                    packet.AppendFormat("{0}.{1}.{2}.{3}", itemInstanceDto.Amount.ToString(), itemInstanceDto.Rarity.ToString(), itemInstanceDto.Upgrade.ToString(),
                        itemInstanceDto.GetCarvedRunesInformation(true));
                    break;

                case InventoryType.Specialist:
                    packet.AppendFormat("{0}.0.0", itemInstanceDto.Upgrade.ToString());
                    break;

                default:
                    packet.AppendFormat("{0}.0.0.0", itemInstanceDto.Amount.ToString());
                    break;
            }

            return packet.ToString();
        }

        public static string GenerateStashSubPacket(GameItemInstance itemInstance, short slot) => GenerateStashSubPacket(itemInstance, itemInstance.GameItem.Type, slot);

        private static string GenerateStashPacketContent(this WarehouseItem warehouseItem) => GenerateStashSubPacket(warehouseItem.ItemInstance, warehouseItem.Slot);

        #endregion

        #region PartnerWarehouse

        public static void SendAddPartnerWarehouseItem(this IClientSession session, PartnerWarehouseItem item) => session.SendPacket(session.GenerateAddPartnerWarehouseItem(item));

        public static void SendRemovePartnerWarehouseItem(this IClientSession session, short slot) => session.SendPacket(session.GenerateRemovePartnerWarehouseItem(slot));

        private static string GeneratePStashPacketContent(this PartnerWarehouseItem warehouseItem) => GenerateStashSubPacket(warehouseItem.ItemInstance, warehouseItem.Slot);

        public static void RefreshPartnerWarehouseItems(this IClientSession session)
        {
            session.SendPacket(session.GeneratePStashAll());
            session.PlayerEntity.IsPartnerWarehouseOpen = true;
        }

        public static string GeneratePStashAll(this IClientSession session)
        {
            var header = new StringBuilder($"pstash_all {session.PlayerEntity.GetPartnerWarehouseSlots()}");
            IEnumerable<PartnerWarehouseItem> items = session.PlayerEntity.PartnerWarehouseItems();
            foreach (PartnerWarehouseItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                header.Append(' ');
                header.Append(item.GeneratePStashPacketContent());
            }

            return header.ToString();
        }

        public static string GenerateAddPartnerWarehouseItem(this IClientSession session, PartnerWarehouseItem item) => $"pstash {item.GeneratePStashPacketContent()}";

        public static string GenerateRemovePartnerWarehouseItem(this IClientSession session, short slot) => $"pstash {slot.ToString()}.-1.0.0.0";

        #endregion
    }
}