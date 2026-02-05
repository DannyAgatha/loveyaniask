using System;
using System.Collections.Concurrent;
using WingsEmu.Game.Managers;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class RevivalManager : IRevivalManager
{
    private readonly ConcurrentDictionary<long, Guid> _pendentRevivals = new();

    public Guid RegisterRevival(long id)
    {
        var newGuid = Guid.NewGuid();
        bool added = _pendentRevivals.TryAdd(id, newGuid);
        return added ? newGuid : default;
    }

    public bool UnregisterRevival(long id, Guid guid)
        => _pendentRevivals.TryGetValue(id, out Guid storedGuid) && storedGuid == guid && _pendentRevivals.TryRemove(id, out _);

    public bool UnregisterRevival(long id) => _pendentRevivals.TryRemove(id, out _);

    public void TryUnregisterRevival(long id)
    {
        _pendentRevivals.TryRemove(id, out _);
    }
}