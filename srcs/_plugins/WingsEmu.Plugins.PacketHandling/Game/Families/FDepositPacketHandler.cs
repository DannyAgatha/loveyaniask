using System.Threading.Tasks;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FDepositPacketHandler : GenericGamePacketHandlerBase<FDepositPacket>
{
    private readonly GeneralServerConfiguration _generalServerConfiguration;
    
    public FDepositPacketHandler(GeneralServerConfiguration generalServerConfiguration)
    {
        _generalServerConfiguration = generalServerConfiguration;
    }
    protected override async Task HandlePacketAsync(IClientSession session, FDepositPacket packet)
    {
        if (packet.Inventory == InventoryType.EquippedItems)
        {
            return;
        }

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(packet.SourceSlot, packet.Inventory);
        if (item == null || packet.Amount < 1 || _generalServerConfiguration.MaxItemAmount < packet.Amount)
        {
            return;
        }

        await session.EmitEventAsync(new FamilyWarehouseAddItemEvent
        {
            Item = item,
            Amount = packet.Amount,
            DestinationSlot = packet.DestinationSlot
        });
    }
}