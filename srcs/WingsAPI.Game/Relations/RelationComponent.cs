using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.DTOs.Relations;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Game.Relations;

public class RelationComponent : IRelationComponent
{
    private const int FRIEND_LIST_LIMIT = 50;

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<CharacterRelationType, List<CharacterRelationDTO>> _relations = new();
    private readonly Dictionary<long, CharacterRelationDTO> _relationsByTargetCharacterId = new();

    public IReadOnlyList<CharacterRelationDTO> GetRelations()
    {
        _lock.EnterReadLock();
        try
        {
            return _relations.Values.SelectMany(s => s).ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerable<CharacterRelationDTO> GetFriendRelations()
    {
        _lock.EnterReadLock();
        try
        {
            if (!_relations.TryGetValue(CharacterRelationType.Friend, out List<CharacterRelationDTO> relations))
            {
                return Array.Empty<CharacterRelationDTO>();
            }

            return relations;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerable<CharacterRelationDTO> GetBlockedRelations()
    {
        _lock.EnterReadLock();
        try
        {
            if (!_relations.TryGetValue(CharacterRelationType.Blocked, out List<CharacterRelationDTO> relations))
            {
                return Array.Empty<CharacterRelationDTO>();
            }

            return relations;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool IsBlocking(long targetId)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_relationsByTargetCharacterId.TryGetValue(targetId, out CharacterRelationDTO relationDto))
            {
                return false;
            }

            return relationDto.RelationType == CharacterRelationType.Blocked;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool IsFriend(long targetId)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_relationsByTargetCharacterId.TryGetValue(targetId, out CharacterRelationDTO relationDto))
            {
                return false;
            }

            return relationDto.RelationType == CharacterRelationType.Friend;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool IsMarried(long targetId)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_relationsByTargetCharacterId.TryGetValue(targetId, out CharacterRelationDTO relationDto))
            {
                return false;
            }

            return relationDto.RelationType == CharacterRelationType.Spouse;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool IsFriendsListFull()
    {
        _lock.EnterReadLock();
        try
        {
            if (!_relations.TryGetValue(CharacterRelationType.Friend, out List<CharacterRelationDTO> list) || list == null)
            {
                return false;
            }

            return list.Count >= FRIEND_LIST_LIMIT;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void AddRelation(CharacterRelationDTO relation)
    {
        _lock.EnterWriteLock();
        try
        {
            _relationsByTargetCharacterId.TryAdd(relation.RelatedCharacterId, relation);
            if (!_relations.TryGetValue(relation.RelationType, out List<CharacterRelationDTO> relations))
            {
                _relations[relation.RelationType] = relations = new List<CharacterRelationDTO>();
            }

            relations.Add(relation);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveRelation(long targetCharacterId, CharacterRelationType relationType)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_relationsByTargetCharacterId.TryGetValue(targetCharacterId, out CharacterRelationDTO relationDto))
            {
                return;
            }

            _relationsByTargetCharacterId.Remove(targetCharacterId, out _);
            if (!_relations.TryGetValue(relationType, out List<CharacterRelationDTO> relations) || relations == null)
            {
                return;
            }

            relations.Remove(relationDto);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}