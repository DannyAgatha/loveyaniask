using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Groups.Events;

public class GroupActionEvent : PlayerEvent
{
    public GroupRequestType RequestType { get; init; }
    public long CharacterId { get; init; }
}