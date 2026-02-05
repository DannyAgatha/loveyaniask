using System.Collections.Generic;
using WingsEmu.Game.Characters;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Groups;

public class GroupFactory : IGroupFactory
{
    private readonly IGroupManager _groupManager;

    public GroupFactory(IGroupManager groupManager) => _groupManager = groupManager;

    public PlayerGroup CreateGroup(byte slots, long ownerId) => CreateGroup(slots, new List<IPlayerEntity>(), ownerId);

    public PlayerGroup CreateGroup(byte slots, List<IPlayerEntity> characters, long ownerId) => new PlayerGroup(_groupManager.GetNextGroupId(), slots, characters, ownerId);

    public PlayerGroup CreateGroup(byte slots, List<IPlayerEntity> characters, long ownerId, GroupSharingType type) =>
        new PlayerGroup(_groupManager.GetNextGroupId(), slots, characters, ownerId, type);
}