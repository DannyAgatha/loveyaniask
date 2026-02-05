using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceRemoveItemsEventHandler : IAsyncEventProcessor<TimeSpaceRemoveItemsEvent>
{
    public async Task HandleAsync(TimeSpaceRemoveItemsEvent e, CancellationToken cancellation)
    {
        IEnumerable<TimeSpaceMapItem> items = e.ItemsToRemove;
        foreach (TimeSpaceMapItem item in items)
        {
            if (!item.MapInstance.RemoveDrop(item.TransportId))
            {
                continue;
            }

            item.BroadcastOut();
        }
    }
}