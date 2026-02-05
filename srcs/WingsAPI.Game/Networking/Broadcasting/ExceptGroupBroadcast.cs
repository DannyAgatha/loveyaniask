using WingsEmu.Game.Groups;

namespace WingsEmu.Game.Networking.Broadcasting;

public class ExceptGroupBroadcast : IBroadcastRule
{
    private readonly long _groupId;

    public ExceptGroupBroadcast(PlayerGroup playerGroup) => _groupId = playerGroup.GroupId;

    public bool Match(IClientSession session) => session.PlayerEntity.GetGroup()?.GroupId != _groupId;
}