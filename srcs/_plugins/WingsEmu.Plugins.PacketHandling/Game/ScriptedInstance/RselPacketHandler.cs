using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class RSelPacketHandler : GenericGamePacketHandlerBase<RSelPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, RSelPacket packet)
    {
        await session.EmitEventAsync(new TimeSpaceSelectRewardEvent
        {
            SendRepayPacket = true
        });
    }
}