using System.Threading.Tasks;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FWithdrawPacketHandler : GenericGamePacketHandlerBase<FWithdrawPacket>
{
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public FWithdrawPacketHandler(GeneralServerConfiguration generalServerConfiguration)
    {
        _generalServerConfiguration = generalServerConfiguration;
    }
    protected override async Task HandlePacketAsync(IClientSession session, FWithdrawPacket packet)
    {
        if (packet.Amount < 1 || _generalServerConfiguration.MaxItemAmount < packet.Amount)
        {
            return;
        }

        await session.EmitEventAsync(new FamilyWarehouseWithdrawItemEvent
        {
            Slot = packet.Slot,
            Amount = packet.Amount
        });
    }
}