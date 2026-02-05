using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class WarehouseAddPacketHandler : GenericGamePacketHandlerBase<DepositPacket>
{
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public WarehouseAddPacketHandler(GeneralServerConfiguration generalServerConfiguration)
    {
        _generalServerConfiguration = generalServerConfiguration;
    }
    
    protected override async Task HandlePacketAsync(IClientSession session, DepositPacket packet)
    {
        if (packet.Inventory == InventoryType.EquippedItems)
        {
            return;
        }

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(packet.Slot, packet.Inventory);
        if (item == null || packet.Amount < 1 || packet.Amount > _generalServerConfiguration.MaxItemAmount)
        {
            return;
        }
        
        if (packet.PartnerBackpack)
        {
            await session.EmitEventAsync(new PartnerWarehouseDepositEvent(packet.Inventory, packet.Slot, packet.Amount, packet.NewSlot));
            return;
        }

        await session.EmitEventAsync(new AccountWarehouseAddItemEvent
        {
            Item = item,
            Amount = packet.Amount,
            SlotDestination = packet.NewSlot
        });
    }
}