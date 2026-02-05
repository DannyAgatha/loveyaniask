using WingsEmu.Game.Groups;

namespace WingsEmu.Game.Networking.Broadcasting;

public class GroupBroadcast : IBroadcastRule
{
    private readonly long _groupId;

    public GroupBroadcast(PlayerGroup playerGroup) => _groupId = playerGroup.GroupId;

    public bool Match(IClientSession session)
    {
        PlayerGroup group = session.PlayerEntity.GetGroup();
        return group != null && group.GroupId == _groupId;
    }
}