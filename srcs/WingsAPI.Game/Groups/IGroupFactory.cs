using System.Collections.Generic;
using WingsEmu.Game.Characters;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Groups;

public interface IGroupFactory
{
    PlayerGroup CreateGroup(byte slots, long ownerId);
    PlayerGroup CreateGroup(byte slots, List<IPlayerEntity> characters, long ownerId);
    PlayerGroup CreateGroup(byte slots, List<IPlayerEntity> characters, long ownerId, GroupSharingType type);
}