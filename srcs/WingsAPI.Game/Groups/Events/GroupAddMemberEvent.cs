using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Groups.Events;

public class GroupAddMemberEvent : PlayerEvent
{
    public GroupAddMemberEvent(IPlayerEntity newMember) => NewMember = newMember;

    public IPlayerEntity NewMember { get; }
}