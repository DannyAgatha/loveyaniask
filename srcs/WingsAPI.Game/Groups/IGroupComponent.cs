using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Groups;

public interface IGroupComponent
{
    public long GetGroupId();
    public PlayerGroup GetGroup();

    public void AddMember(IPlayerEntity member);
    public void RemoveMember(IPlayerEntity member);

    public void SetGroup(PlayerGroup playerGroup);
    public void RemoveGroup();

    public bool IsInGroup();
    public bool IsLeaderOfGroup(long characterId);
    public bool IsGroupFull();
}