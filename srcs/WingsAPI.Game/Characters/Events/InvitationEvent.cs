using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class InvitationEvent : PlayerEvent
{
    public InvitationEvent(long targetCharacterId, InvitationType type)
    {
        TargetCharacterId = targetCharacterId;
        Type = type;
    }

    public long TargetCharacterId { get; }

    public InvitationType Type { get; }
}