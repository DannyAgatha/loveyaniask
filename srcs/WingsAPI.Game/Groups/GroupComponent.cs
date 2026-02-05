using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Groups;

public class GroupComponent : IGroupComponent
{
    private PlayerGroup _playerGroup;

    public GroupComponent() => _playerGroup = null;

    public long GetGroupId() => _playerGroup?.GroupId ?? 0;

    public PlayerGroup GetGroup() => _playerGroup;

    public void AddMember(IPlayerEntity member) => _playerGroup?.AddMember(member);

    public void RemoveMember(IPlayerEntity member) => _playerGroup?.RemoveMember(member);

    public void SetGroup(PlayerGroup playerGroup)
    {
        if (_playerGroup != null)
        {
            return;
        }

        _playerGroup = playerGroup;
    }

    public void RemoveGroup()
    {
        _playerGroup = null;
    }

    public bool IsInGroup() => _playerGroup != null;

    public bool IsLeaderOfGroup(long characterId) => _playerGroup != null && _playerGroup.OwnerId == characterId;

    public bool IsGroupFull() => _playerGroup != null && _playerGroup.Members.Count >= _playerGroup.Slots;
}