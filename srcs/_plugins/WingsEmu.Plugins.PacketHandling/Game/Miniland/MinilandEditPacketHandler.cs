using System;
using System.Threading.Tasks;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Miniland;

public class MinilandEditPacketHandler : GenericGamePacketHandlerBase<MlEditPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, MlEditPacket packet)
    {
        if (packet == null)
        {
            return;
        }

        switch (packet.Type)
        {
            case 1:
                if (packet.Parameters == default)
                {
                    return;
                }

                await session.EmitEventAsync(new MinilandIntroEvent(packet.Parameters));
                break;

            case 2:
                if (!Enum.TryParse(packet.Parameters, out MinilandState state))
                {
                    throw new ArgumentOutOfRangeException("", $"Miniland State Type received doesn't equal to any known enum value -> 'OutOfRangeValue': {packet.Parameters}");
                }

                await session.EmitEventAsync(new MinilandStateEvent(state));
                break;
        }
    }
}