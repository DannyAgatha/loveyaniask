using System.Threading;

namespace WingsEmu.Game.Skills;

public interface IWildKeeperComponent
{
    public void IncreaseElementStacks();
    public void DecreaseElementStacks();
    public void ResetElementStacks();
    public short ElementStacks();
}

public class WildKeeperComponent : IWildKeeperComponent
{
    private readonly ReaderWriterLockSlim _lock = new();
    private short _elementsCounter;

    public void IncreaseElementStacks()
    {
        _lock.EnterWriteLock();
        try
        {
            _elementsCounter++;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void DecreaseElementStacks()
    {
        _lock.EnterWriteLock();
        try
        {
            _elementsCounter--;
            if (_elementsCounter < 0)
            {
                _elementsCounter = 0;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void ResetElementStacks()
    {
        _lock.EnterWriteLock();
        try
        {
            _elementsCounter = 0;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public short ElementStacks()
    {
        _lock.EnterReadLock();
        try
        {
            return _elementsCounter;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}