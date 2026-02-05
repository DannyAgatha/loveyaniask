using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class MountBeadGuriHandler : IGuriHandler
{
    public long GuriEffectId => 208;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.User == null)
        {
            Log.Warn($"The guriPacket doesn't contain any value for User. GuriEffectId : {GuriEffectId}");
            return;
        }

        if (!short.TryParse(guriPacket.User.Value.ToString(), out short pearlSlot) || !short.TryParse(guriPacket.Value, out short mountSlot))
        {
            Log.Warn($"The pearlSlot or mountSlot value is null. GuriEffectId : {GuriEffectId}");
            return;
        }

        InventoryItem mount = session.PlayerEntity.GetItemBySlotAndType(mountSlot, InventoryType.Main);
        InventoryItem bead = session.PlayerEntity.GetItemBySlotAndType(pearlSlot, InventoryType.Equipment);

        if (mount == null || bead == null)
        {
            return;
        }

        if (bead.ItemInstance.Type != ItemInstanceType.BoxInstance)
        {
            return;
        }

        GameItemInstance beadItem = bead.ItemInstance;

        if (beadItem.HoldingVNum != 0 && beadItem.HoldingVNum != null)
        {
            return;
        }

        if (beadItem.GameItem.ItemSubType != 4 || mount.ItemInstance.GameItem.ItemType != ItemType.Special || mount.ItemInstance.GameItem.Effect != 1000)
        {
            return;
        }

        beadItem.HoldingVNum = mount.ItemInstance.ItemVNum;
        await session.RemoveItemFromInventory(mount.ItemInstance.ItemVNum);

        session.SendGuriPacket(25);
        session.SendEsfPacket(0);
    }
}