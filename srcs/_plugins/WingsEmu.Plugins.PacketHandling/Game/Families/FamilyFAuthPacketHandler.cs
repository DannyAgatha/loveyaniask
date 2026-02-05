// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FamilyFAuthPacketHandler : GenericGamePacketHandlerBase<FAuthPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FAuthPacket packet)
    {
        if (!Enum.TryParse(packet.AuthorityId.ToString(), out FamilyActionType actionType))
        {
            return;
        }

        await session.EmitEventAsync(new FamilyChangeSettingsEvent
        {
            Authority = packet.MemberType,
            FamilyActionType = actionType,
            Value = packet.Value
        });
    }
}