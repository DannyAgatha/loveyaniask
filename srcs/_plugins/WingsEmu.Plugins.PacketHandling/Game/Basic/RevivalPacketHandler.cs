using System;
using System.Threading.Tasks;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class RevivalPacketHandler : GenericGamePacketHandlerBase<RevivalPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, RevivalPacket packet)
    {
        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (!Enum.IsDefined(typeof(RevivalType), (int)packet.Type))
        {
            throw new ArgumentOutOfRangeException("", $"The RevivalType that was requested isn't defined -> 'ValueRecieved': {packet.Type.ToString()}");
        }

        await session.EmitEventAsync(new RevivalReviveEvent((RevivalType)packet.Type));
    }
}