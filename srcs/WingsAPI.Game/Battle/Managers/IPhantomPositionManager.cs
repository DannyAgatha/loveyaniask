using System;
using System.Collections.Concurrent;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle.Managers;

public interface IPhantomPositionManager
{
    Position? GetPosition(Guid id);
    void AddPosition(Guid id, Position position);
}

public class PhantomPositionManager : IPhantomPositionManager
{
    private readonly ConcurrentDictionary<Guid, Position> _positions = new();

    public Position? GetPosition(Guid id)
    {
        if (!_positions.TryRemove(id, out Position position))
        {
            return null;
        }

        return position;
    }

    public void AddPosition(Guid id, Position position)
    {
        _positions.TryAdd(id, position);
    }
}