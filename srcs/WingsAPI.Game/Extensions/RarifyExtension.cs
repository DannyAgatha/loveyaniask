using System.Threading.Tasks;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Extensions;

public static class RarifyExtension
{
    public static async Task EmitRarifyEvent(this IClientSession sender, InventoryItem invItem, InventoryItem invAmulet, bool isDrop = false, bool isScroll = false)
    {
        RarifyMode mode = RarifyMode.Normal;
        RarifyProtection protection = RarifyProtection.None;

        if (invItem.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }

        GameItemInstance item = invItem.ItemInstance;
        GameItemInstance amulet = invAmulet?.ItemInstance;

        if (isDrop)
        {
            mode = RarifyMode.Drop;
        }

        if (isScroll)
        {
            protection = RarifyProtection.Scroll;
        }

        if (amulet == null)
        {
            await sender.EmitEventAsync(new GamblingEvent(invItem, null, mode, protection));
            return;
        }

        switch (amulet.ItemVNum)
        {
            case (int)ItemVnums.EQ_NORMAL_SCROLL:
                protection = RarifyProtection.Scroll;
                break;
            case (int)ItemVnums.BLESSING_AMULET:
                protection = RarifyProtection.BlessingAmulet;
                break;
            case (int)ItemVnums.PROTECTION_AMULET:
                protection = RarifyProtection.ProtectionAmulet;
                break;
            case (int)ItemVnums.BLESSING_AMULET_DOUBLE:
                protection = RarifyProtection.BlessingAmulet;
                break;
            case (int)ItemVnums.CHAMPION_AMULET:
                protection = RarifyProtection.HeroicAmulet;
                break;
            case (int)ItemVnums.CHAMPION_AMULET_RANDOM:
                protection = RarifyProtection.RandomHeroicAmulet;
                break;

            case (int)ItemVnums.AMULET_INCREASE_NORMAL:
                mode = RarifyMode.Increase;
                break;
            case (int)ItemVnums.CHAMPION_AMULET_INCREASE_1:
            case (int)ItemVnums.CHAMPION_AMULET_INCREASE_2:
                if (item.GameItem.IsHeroic)
                {
                    mode = RarifyMode.Increase;
                }

                break;
        }

        await sender.EmitEventAsync(new GamblingEvent(invItem, invAmulet, mode, protection));
    }
}