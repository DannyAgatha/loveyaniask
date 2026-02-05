// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.Game.Characters;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Groups;

public class PlayerGroup
{
    private readonly List<IPlayerEntity> _characters;
    private readonly ReaderWriterLockSlim _lock = new();
    private int _order;

    public PlayerGroup(int groupId, byte slots, List<IPlayerEntity> members, long ownerId, GroupSharingType sharingMode = GroupSharingType.ByOrder)
    {
        GroupId = groupId;
        Slots = slots;
        _characters = members;
        SharingMode = sharingMode;
        OwnerId = ownerId;
    }

    public int GroupId { get; }

    public GroupSharingType SharingMode { get; set; }

    public byte Slots { get; }

    public IReadOnlyList<IPlayerEntity> Members
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _characters;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }


    public long OwnerId { get; set; }

    public long ArenaKills { get; set; }
    public long ArenaDeaths { get; set; }

    public void AddMember(IPlayerEntity character)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_characters.Contains(character))
            {
                return;
            }

            _characters.Add(character);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveMember(IPlayerEntity character)
    {
        _lock.EnterWriteLock();
        try
        {
            _characters.Remove(character);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public long? GetNextOrderedCharacterId(IPlayerEntity character)
    {
        _lock.EnterReadLock();
        try
        {
            _order++;
            IPlayerEntity[] characters = _characters.Where(x => x != null).ToArray();
            if (_characters.Count == 0) // group seems to be empty
            {
                return null;
            }

            if (_order > _characters.Count - 1) // if order wents out of amount of ppl, reset it -> zero based index
            {
                _order = 0;
            }

            return characters[_order].Id;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}