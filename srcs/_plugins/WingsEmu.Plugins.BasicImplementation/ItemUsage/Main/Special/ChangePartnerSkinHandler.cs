using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class ChangePartnerSkinHandler : IItemHandler
{
    private static readonly int[] PartnersWithSkins = { 317, 318, 319, 2557, 2617, 2618, 2620, 2640, 2673 };
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 305 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IMateEntity partner = session.PlayerEntity.MateComponent.GetMate(x => x.IsTeamMember && x.MateType == MateType.Partner);

        if (partner == null)
        {
            return;
        }

        if (!partner.IsAlive())
        {
            return;
        }

        if (partner.IsUsingSp)
        {
            return;
        }

        if (!PartnersWithSkins.Contains(partner.NpcMonsterVNum))
        {
            return;
        }

        if (partner.Skin == e.Item.ItemInstance.GameItem.Morph)
        {
            return;
        }

        switch (partner.NpcMonsterVNum)
        {
            case 317: // Bob
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_BOB)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 318: // Tom
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_TOM)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 319: // Kliff
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_KLIFF)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 2617: // Frigg
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_FRIGG)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 2618: // Ragnar
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_RAGNAR)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 2620: // Erdimien
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_ERDIMIEN)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 2640: // Jennifer
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_JENNIFER)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
            case 2673: // Yertirand
                if (e.Item.ItemInstance.ItemVNum != (short)ItemVnums.SKIN_FOR_YERTIRAND)
                {
                    return;
                }

                partner.Skin = e.Item.ItemInstance.GameItem.Morph;
                break;
        }

        await session.RemoveItemFromInventory(item: e.Item);
        session.CurrentMapInstance?.Broadcast(partner.GenerateCMode(partner.Skin));
        session.SendCondMate(partner);
    }
}