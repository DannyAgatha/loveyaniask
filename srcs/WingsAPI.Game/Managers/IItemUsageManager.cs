using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WingsEmu.Game.Managers;

public interface IItemUsageManager
{
    int GetLastItemUsed(long characterId);
    void SetLastItemUsed(long characterId, int vNum);
}

public class ItemUsageManager : IItemUsageManager
{
    private readonly ConcurrentDictionary<long, int> _lastItemUsed;

    public ItemUsageManager() => _lastItemUsed = new ConcurrentDictionary<long, int>();

    public int GetLastItemUsed(long characterId) => _lastItemUsed.GetValueOrDefault(characterId);

    public void SetLastItemUsed(long characterId, int vNum)
    {
        _lastItemUsed[characterId] = vNum;
    }
}