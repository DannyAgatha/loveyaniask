using System.Collections.Generic;
using WingsEmu.DTOs.Relations;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Game.Relations;

public interface IRelationComponent
{
    public IReadOnlyList<CharacterRelationDTO> GetRelations();
    public IEnumerable<CharacterRelationDTO> GetFriendRelations();
    public IEnumerable<CharacterRelationDTO> GetBlockedRelations();

    public bool IsBlocking(long targetId);
    public bool IsFriend(long targetId);
    public bool IsMarried(long targetId);
    public bool IsFriendsListFull();
    public void AddRelation(CharacterRelationDTO relation);
    public void RemoveRelation(long targetCharacterId, CharacterRelationType relationType);
}