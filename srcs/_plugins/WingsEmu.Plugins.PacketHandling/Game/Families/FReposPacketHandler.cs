using System.Threading.Tasks;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FReposPacketHandler : GenericGamePacketHandlerBase<FReposPacket>
{
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public FReposPacketHandler(GeneralServerConfiguration generalServerConfiguration)
    {
        _generalServerConfiguration = generalServerConfiguration;
    }
    protected override async Task HandlePacketAsync(IClientSession session, FReposPacket fReposPacket)
    {
        if (fReposPacket.Amount < 1 || _generalServerConfiguration.MaxItemAmount < fReposPacket.Amount)
        {
            return;
        }

        await session.EmitEventAsync(new FamilyWarehouseMoveItemEvent
        {
            OldSlot = fReposPacket.OldSlot,
            Amount = fReposPacket.Amount,
            NewSlot = fReposPacket.NewSlot
        });
    }
}