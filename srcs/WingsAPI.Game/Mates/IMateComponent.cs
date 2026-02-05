using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WingsEmu.Game.Mates;

public interface IMateComponent
{
    IReadOnlyList<IMateEntity> TeamMembers();
    IReadOnlyList<IMateEntity> TeamMembers(Func<IMateEntity, bool> predicate);
    IReadOnlyList<IMateEntity> GetMates();
    IReadOnlyList<IMateEntity> GetMates(Func<IMateEntity, bool> predicate);
    IMateEntity GetMate(Func<IMateEntity, bool> predicate);
    IMateEntity GetTeamMember(Func<IMateEntity, bool> predicate);
    void AddMate(IMateEntity mateEntity);
    void RemoveMate(IMateEntity mateEntity);
}

public class MateComponent : IMateComponent
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly List<IMateEntity> _mates = new();

    public IReadOnlyList<IMateEntity> TeamMembers()
    {
        return GetMates(x => x.IsTeamMember);
    }

    public IReadOnlyList<IMateEntity> TeamMembers(Func<IMateEntity, bool> predicate)
    {
        return GetMates(x => x.IsTeamMember && predicate(x));
    }

    public IReadOnlyList<IMateEntity> GetMates()
    {
        _lock.EnterReadLock();
        try
        {
            return _mates.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IReadOnlyList<IMateEntity> GetMates(Func<IMateEntity, bool> predicate)
    {
        _lock.EnterReadLock();
        try
        {
            return _mates.FindAll(x => x != null && (predicate == null || predicate(x)));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IMateEntity GetMate(Func<IMateEntity, bool> predicate)
    {
        _lock.EnterReadLock();
        try
        {
            return _mates.FirstOrDefault(x => x != null && (predicate == null || predicate(x)));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IMateEntity GetTeamMember(Func<IMateEntity, bool> predicate)
    {
        return GetMate(x => x.IsTeamMember && predicate(x));
    }

    public void AddMate(IMateEntity mateEntity)
    {
        _lock.EnterWriteLock();
        try
        {
            _mates.Add(mateEntity);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveMate(IMateEntity mateEntity)
    {
        _lock.EnterWriteLock();
        try
        {
            _mates.Remove(mateEntity);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}