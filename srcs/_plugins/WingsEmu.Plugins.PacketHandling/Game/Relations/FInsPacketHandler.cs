using System;
using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class FInsPacketHandler : GenericGamePacketHandlerBase<FInsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FInsPacket packet)
    {
        if (!Enum.TryParse(packet.Type.ToString(), out FInsPacketType type))
        {
            return;
        }
        
        if (session.PlayerEntity.MapInstance.IsAct6PvpInstance)
        {
            return;
        }

        await session.EmitEventAsync(new RelationFriendEvent
        {
            RequestType = type,
            CharacterId = packet.CharacterId
        });
    }
}