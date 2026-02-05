using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.Families
{
    public static class FamilyWarehousePacketExtension
    {
        public static void SendFamilyWarehouseLogs(this IClientSession session, IList<FamilyWarehouseLogEntryDto> logs)
        {
            session.SendPacket(session.GenerateFamilyWarehouseLogs(logs));
        }

        public static void SendFamilyWarehouseItems(this IClientSession session, IItemsManager itemsManager, int warehouseSize, IEnumerable<FamilyWarehouseItemDto> items) =>
            session.SendPacket(session.GenerateFStashAll(itemsManager, warehouseSize, items));

        public static void SendFamilyWarehouseAddItem(this IClientSession session, IItemsManager itemsManager, FamilyWarehouseItemDto item)
        {
            session.SendPacket(item.GenerateFStashAdd(itemsManager));
        }

        public static void SendFamilyWarehouseRemoveItem(this IClientSession session, FamilyWarehouseItemDto item)
        {
            session.SendPacket(session.GenerateFStashRemove(item));
        }

        public static void SendFamilyWarehouseRemoveItem(this IClientSession session, short slot)
        {
            session.SendPacket(session.GenerateFStashRemove(slot));
        }

        private static string GenerateFStashPacketContent(this FamilyWarehouseItemDto warehouseItem, IItemsManager itemsManager) =>
            WarehousePacketExtensions.GenerateStashSubPacket(itemsManager, warehouseItem.ItemInstance, warehouseItem.Slot);

        public static string GenerateFStashAll(this IClientSession session, IItemsManager itemsManager, int warehouseSize, IEnumerable<FamilyWarehouseItemDto> items)
        {
            var packet = new StringBuilder($"f_stash_all {warehouseSize.ToString()}");
            foreach (FamilyWarehouseItemDto item in items)
            {
                packet.Append(' ');
                packet.Append(item.GenerateFStashPacketContent(itemsManager));
            }

            return packet.ToString();
        }

        public static string GenerateFStashAdd(this FamilyWarehouseItemDto item, IItemsManager itemsManager) => $"f_stash {item.GenerateFStashPacketContent(itemsManager)}";

        public static string GenerateFStashRemove(this IClientSession session, FamilyWarehouseItemDto item) => session.GenerateFStashRemove(item.Slot);

        public static string GenerateFStashRemove(this IClientSession session, short slot) => $"f_stash {slot.ToString()}.-1.0.0.0";

        public static string GenerateFamilyWarehouseLogs(this IClientSession session, IList<FamilyWarehouseLogEntryDto> logs)
        {
            StringBuilder packet = new("fslog_stc -1");
            DateTime currentTime = DateTime.UtcNow;

            for (int i = logs.Count - 1; i >= 0; i--)
            {
                FamilyWarehouseLogEntryDto log = logs[i];
                packet.AppendFormat(" {0}|{1}|{2}|{3}|{4}", ((byte)log.Type).ToString(), log.CharacterName, log.ItemVnum.ToString(), log.Amount.ToString(),
                    Math.Floor((currentTime - log.DateOfLog).TotalHours).ToString(CultureInfo.InvariantCulture));
            }

            return packet.ToString();
        }
    }
}