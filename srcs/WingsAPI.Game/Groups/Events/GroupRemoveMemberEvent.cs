using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Groups.Events;

public class RemoveMemberFromGroupEvent : PlayerEvent
{
    public RemoveMemberFromGroupEvent(IPlayerEntity memberToRemove) => MemberToRemove = memberToRemove;

    public IPlayerEntity MemberToRemove { get; }
}