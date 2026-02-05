using System;
using System.Text;
using PhoenixLib.MultiLanguage;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Game.Extensions.ItemExtension.Item
{
    public static class ItemExtension
    {
        public static bool IsTimeSpaceStone(this IGameItem item) => item.Data[0] == 900;

        public static bool IsTimeSpaceChest(this IGameItem gameItem) => gameItem.Data[0] == 4;

        public static string GetItemName(this IGameItem gameItem, IGameLanguageService gameLanguage, RegionLanguageType regionLanguageType)
            => gameLanguage.GetLanguage(GameDataType.Item, gameItem.Name, regionLanguageType);

        public static bool ShouldSendAmuletPacket(this IClientSession session, EquipmentType type) =>
            type != EquipmentType.CostumeHat && type != EquipmentType.CostumeSuit && type != EquipmentType.WeaponSkin;

        public static InventoryItem CreateInventoryItem(this IGameItemInstanceFactory instanceFactory, int vnum) => new()
        {
            ItemInstance = instanceFactory.CreateItem(vnum)
        };

        public static bool IsRenegadeSpecialist(this IClientSession session, GameItemInstance specialist, GameItemInstance fairy)
        {
            return specialist != null && specialist.GameItem.Id == (short)ItemVnums.RENEGADE_SPECIALIST_CARD &&
                fairy != null && (fairy.GameItem.Element == (short)ElementType.Light || fairy.GameItem.Element == (short)ElementType.Shadow);
        }
        
        public static string ToReadableString(this TimeSpan timeSpan)
        {
            var formattedTime = new StringBuilder();

            if (timeSpan.Days > 0)
            {
                formattedTime.Append($"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")}, ");
            }

            if (timeSpan.Hours > 0)
            {
                formattedTime.Append($"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")}, ");
            }

            if (timeSpan.Minutes > 0)
            {
                formattedTime.Append($"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")}, ");
            }

            if (timeSpan.Seconds > 0)
            {
                formattedTime.Append($"{timeSpan.Seconds} second{(timeSpan.Seconds > 1 ? "s" : "")}");
            }

            return formattedTime.ToString().TrimEnd(',', ' ');
        }
    }
}