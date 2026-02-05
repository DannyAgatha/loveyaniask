using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.Core.Extensions;
using WingsEmu.Game._enum;
using WingsEmu.Game.Extensions;

namespace WingsEmu.Game.Buffs;

public class BuffComponent : IBuffComponent
{
    private readonly ConcurrentDictionary<Guid, Buff> _buffByBuffId = new();
    private readonly ConcurrentDictionary<int, Buff> _buffByCardId = new();
    private readonly ConcurrentDictionary<int, Buff> _buffByGroupId = new();
    private readonly ConcurrentDictionary<BuffGroup, HashSet<Buff>> _buffByTypes = new();
    private readonly List<Buff> _buffs = new();
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    public bool HasAnyBuff() => _buffs.Count != 0;

    public IReadOnlyList<Buff> GetAllBuffs()
    {
        _lock.EnterReadLock();
        try
        {
            return _buffs.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public ICollection<int> GetAllBuffsId() => _buffByCardId.Keys;

    public IReadOnlyList<Buff> GetAllBuffs(Func<Buff, bool> predicate)
    {
        _lock.EnterReadLock();
        try
        {
            return _buffs.FindAll(x => x != null && (predicate == null || predicate(x)));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void AddBuff(Buff buff)
    {
        if (buff == null)
        {
            return;
        }

        _lock.EnterWriteLock();
        try
        {
            if (_buffByCardId.ContainsKey(buff.CardId))
            {
                return;
            }

            HashSet<Buff> buffWithSameType = _buffByTypes.GetOrDefault(buff.BuffGroup);
            if (buffWithSameType == null)
            {
                buffWithSameType = new HashSet<Buff>();
                _buffByTypes[buff.BuffGroup] = buffWithSameType;
            }

            buffWithSameType.Add(buff);
            _buffs.Add(buff);
            _buffByCardId.TryAdd(buff.CardId, buff);
            _buffByBuffId.TryAdd(buff.BuffId, buff);
            _buffByGroupId.TryAdd(buff.GroupId, buff);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Buff GetBuff(int cardId)
    {
        _lock.EnterReadLock();
        try
        {
            return !_buffByCardId.TryGetValue(cardId, out Buff buff) ? null : buff;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Buff GetBuffByGroupId(int groupId)
    {
        _lock.EnterReadLock();
        try
        {
            return !_buffByGroupId.TryGetValue(groupId, out Buff buff) ? null : buff;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool HasBuff(BuffGroup buffGroup) => _buffByTypes.TryGetValue(buffGroup, out HashSet<Buff> buff) && buff.Count > 0;
    public bool HasBuff(int cardId) => _buffByCardId.ContainsKey(cardId);
    public bool HasBuff(Guid buffId) => _buffByBuffId.ContainsKey(buffId);

    public void RemoveBuff(Guid buffId)
    {
        _lock.EnterWriteLock();
        try
        {
            _buffByBuffId.Remove(buffId, out Buff buff);
            if (buff == null)
            {
                return;
            }

            _buffs.Remove(buff);
            _buffByCardId.TryRemove(buff.CardId, out _);
            _buffByTypes.GetOrDefault(buff.BuffGroup)?.Remove(buff);
            _buffByGroupId.TryRemove(buff.GroupId, out _);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void ClearNonPersistentBuffs()
    {
        _lock.EnterWriteLock();
        try
        {
            IEnumerable<Buff> buffsToRemove = _buffByCardId.Values.ToArray();
            buffsToRemove = buffsToRemove.Where(s => !s.IsSavingOnDisconnect());
            foreach (Buff buff in buffsToRemove)
            {
                _buffByTypes[buff.BuffGroup].Remove(buff);
                _buffByBuffId.TryRemove(buff.BuffId, out _);
                _buffByCardId.TryRemove(buff.CardId, out _);
                _buffByGroupId.TryRemove(buff.GroupId, out _);
                _buffs.Remove(buff);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}