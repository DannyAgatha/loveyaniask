using System;
using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class CspePacketHandler : GenericGamePacketHandlerBase<CspePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, CspePacket packet)
    {
        session.PlayerEntity.Bubble = DateTime.MinValue;
        session.PlayerEntity.RemoveBubble();
    }
}