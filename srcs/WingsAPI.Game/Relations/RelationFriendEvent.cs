using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Game.Relations;

public class RelationFriendEvent : PlayerEvent
{
    public FInsPacketType RequestType { get; init; }
    public long CharacterId { get; init; }
}