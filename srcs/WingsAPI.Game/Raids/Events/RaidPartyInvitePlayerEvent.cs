using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidPartyInvitePlayerEvent : PlayerEvent
{
    public RaidPartyInvitePlayerEvent(long targetId) => TargetId = targetId;

    public long TargetId { get; }
}