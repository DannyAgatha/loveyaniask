using System.Threading;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Act4;

public interface IAct4FlagManager
{
    MapLocation AngelFlag { get; }
    MapLocation DemonFlag { get; }

    void SetAngelFlag(MapLocation mapLocation);
    void SetDemonFlag(MapLocation mapLocation);

    void RemoveAngelFlag();
    void RemoveDemonFlag();
}

public class Act4FlagManager : IAct4FlagManager
{
    private readonly ReaderWriterLockSlim _lock = new();

    private MapLocation _angelFlag;
    private MapLocation _demonFlag;

    public MapLocation AngelFlag
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _angelFlag;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public MapLocation DemonFlag
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _demonFlag;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void SetAngelFlag(MapLocation mapLocation)
    {
        _lock.EnterWriteLock();
        try
        {
            _angelFlag = mapLocation;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void SetDemonFlag(MapLocation mapLocation)
    {
        _lock.EnterWriteLock();
        try
        {
            _demonFlag = mapLocation;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveAngelFlag()
    {
        _lock.EnterWriteLock();
        try
        {
            _angelFlag = null;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveDemonFlag()
    {
        _lock.EnterWriteLock();
        try
        {
            _demonFlag = null;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}