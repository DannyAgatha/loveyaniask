using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class MvePacketHandler : GenericGamePacketHandlerBase<MvePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, MvePacket mvePacket)
    {
        if (mvePacket.Slot == mvePacket.DestinationSlot && mvePacket.InventoryType == mvePacket.DestinationInventoryType)
        {
            return;
        }

        if (mvePacket.DestinationSlot > session.PlayerEntity.GetInventorySlots(inventoryType: mvePacket.DestinationInventoryType))
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (mvePacket.InventoryType != InventoryType.Costume &&
            mvePacket.InventoryType != InventoryType.Specialist &&
            mvePacket.InventoryType != InventoryType.Equipment)
        {
            return;
        }

        if (mvePacket.DestinationInventoryType != InventoryType.Costume &&
            mvePacket.DestinationInventoryType != InventoryType.Specialist &&
            mvePacket.DestinationInventoryType != InventoryType.Equipment)
        {
            return;
        }

        InventoryItem sourceItem = session.PlayerEntity.GetItemBySlotAndType(mvePacket.Slot, mvePacket.InventoryType);
        if ((sourceItem == null || sourceItem.ItemInstance.GameItem.ItemType != ItemType.Specialist) && (sourceItem == null || sourceItem.ItemInstance.GameItem.ItemType != ItemType.Fashion))
        {
            return;
        }

        await session.EmitEventAsync(new InventoryMoveItemEvent(mvePacket.InventoryType, mvePacket.Slot, (short)sourceItem.ItemInstance.Amount,
            mvePacket.DestinationSlot, mvePacket.DestinationInventoryType));
    }
}