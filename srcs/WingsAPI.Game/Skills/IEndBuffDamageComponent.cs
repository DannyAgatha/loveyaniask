using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WingsEmu.Game.Skills;

public interface IEndBuffDamageComponent
{
    public IReadOnlyDictionary<short, int> EndBuffDamages { get; }

    public void AddEndBuff(short buffVnum, int damage);
    public int DecreaseDamageEndBuff(short buffVnum, int damage);
    public void RemoveEndBuffDamage(short buffVnum);
}

public class EndBuffDamageComponent : IEndBuffDamageComponent
{
    private readonly ConcurrentDictionary<short, int> _buffs = new();
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    public IReadOnlyDictionary<short, int> EndBuffDamages => _buffs;

    public void AddEndBuff(short buffVnum, int damage)
    {
        _lock.EnterWriteLock();
        try
        {
            _buffs.TryAdd(buffVnum, damage);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public int DecreaseDamageEndBuff(short buffVnum, int damage)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_buffs.TryGetValue(buffVnum, out int damageCounter))
            {
                return 0;
            }

            damageCounter -= damage;
            _buffs[buffVnum] = damageCounter;
            return damageCounter;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveEndBuffDamage(short buffVnum)
    {
        _lock.EnterWriteLock();
        try
        {
            _buffs.TryRemove(buffVnum, out _);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}