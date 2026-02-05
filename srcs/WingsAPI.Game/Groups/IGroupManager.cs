// NosEmu
// 


using WingsEmu.Game._ECS;
using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Groups;

public interface IGroupManager : ITickProcessable
{
    int GetNextGroupId();
    void JoinGroup(PlayerGroup group, IPlayerEntity character);
    void RemoveGroup(PlayerGroup group, IPlayerEntity character);
    void AddMemberGroup(PlayerGroup group, IPlayerEntity character);
    void RemoveMemberGroup(PlayerGroup group, IPlayerEntity character);
    void ChangeLeader(PlayerGroup group, long newLeaderId);
}