using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class SortOpenPacketHandler : GenericGamePacketHandlerBase<SortOpenPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, SortOpenPacket packet)
    {
        InventoryItem[] specialists = session.PlayerEntity.GetItemsByInventoryType(InventoryType.Specialist).OrderBy(x => x?.ItemInstance.ItemVNum).ToArray();
        InventoryItem[] costumes = session.PlayerEntity.GetItemsByInventoryType(InventoryType.Costume).OrderBy(x => x?.ItemInstance.ItemVNum).ToArray();

        if (!costumes.Any() && !specialists.Any())
        {
            return;
        }

        if (specialists.Any())
        {
            foreach (InventoryItem specialist in specialists)
            {
                short slot = session.PlayerEntity.GetNextInventorySlot(InventoryType.Specialist);
                await session.EmitEventAsync(new InventoryMoveItemEvent(InventoryType.Specialist, specialist.Slot, 1, slot, InventoryType.Specialist, false));
            }

            session.SendSortedItems(InventoryType.Specialist);
        }

        if (costumes.Any())
        {
            foreach (InventoryItem costume in costumes)
            {
                short slot = session.PlayerEntity.GetNextInventorySlot(InventoryType.Costume);
                await session.EmitEventAsync(new InventoryMoveItemEvent(InventoryType.Costume, costume.Slot, 1, slot, InventoryType.Costume, false));
            }

            session.SendSortedItems(InventoryType.Costume);
        }
    }
}