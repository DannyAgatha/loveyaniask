using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceRemoveItemsEvent : IAsyncEvent
{
    public IEnumerable<TimeSpaceMapItem> ItemsToRemove { get; init; }
}