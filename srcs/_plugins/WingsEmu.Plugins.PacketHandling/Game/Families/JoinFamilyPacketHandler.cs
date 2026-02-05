// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class JoinFamilyPacketHandler : GenericGamePacketHandlerBase<JoinFamilyPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, JoinFamilyPacket joinFamilyPacket)
    {
        if (!Enum.TryParse(joinFamilyPacket.Type.ToString(), out FamilyJoinType type))
        {
            return;
        }

        await session.EmitEventAsync(new FamilyInviteResponseEvent(type, joinFamilyPacket.CharacterId));
    }
}