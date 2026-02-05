using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class FairyBeadGuriHandler : IGuriHandler
{
    public long GuriEffectId => 209;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.Data != 0)
        {
            return;
        }

        if (guriPacket.User == null)
        {
            return;
        }

        if (!short.TryParse(guriPacket.User.Value.ToString(), out short beadSlot))
        {
            return;
        }

        if (!short.TryParse(guriPacket.Value, out short fairySlot))
        {
            return;
        }

        InventoryItem fairy = session.PlayerEntity.GetItemBySlotAndType(fairySlot, InventoryType.Equipment);
        InventoryItem bead = session.PlayerEntity.GetItemBySlotAndType(beadSlot, InventoryType.Equipment);

        if (fairy == null || bead == null)
        {
            return;
        }

        if (bead.ItemInstance.Type != ItemInstanceType.BoxInstance)
        {
            return;
        }

        if (fairy.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }

        GameItemInstance beadItem = bead.ItemInstance;
        GameItemInstance fairyItem = fairy.ItemInstance;

        session.SendGuriPacket(27);
        session.SendEsfPacket(0);

        if (beadItem.HoldingVNum != 0 && beadItem.HoldingVNum != null)
        {
            return;
        }

        if (fairyItem.GameItem.IsLimited || fairyItem.GameItem.Id >= (short)ItemVnums.FIRE_FAIRY && fairyItem.GameItem.Id <= (short)ItemVnums.SHADOW_FAIRY)
        {
            return;
        }

        if (beadItem.GameItem.ItemSubType != 5 || fairyItem.GameItem.ItemType != ItemType.Jewelry || fairyItem.GameItem.ItemSubType != 3)
        {
            return;
        }

        beadItem.HoldingVNum = fairyItem.ItemVNum;
        beadItem.ElementRate = fairyItem.ElementRate;
        beadItem.Xp = fairyItem.Xp;

        await session.RemoveItemFromInventory(item: fairy);
    }
}