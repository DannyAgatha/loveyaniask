using System.Collections.Generic;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game._ECS.Systems;

public interface IDropSystem
{
    IReadOnlyList<MapItem> Drops { get; }
    void AddDrop(MapItem item);
    bool RemoveDrop(long dropId);
    bool HasDrop(long dropId);
    MapItem GetDrop(long dropId);
}