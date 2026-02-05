using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.Maps;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class SRemoveItemsEventConverter : ScriptedEventConverter<SRemoveItemsEvent>
{
    private readonly Dictionary<Guid, TimeSpaceMapItem> _items;

    public SRemoveItemsEventConverter(Dictionary<Guid, TimeSpaceMapItem> items) => _items = items;

    protected override IAsyncEvent Convert(SRemoveItemsEvent e)
    {
        List<TimeSpaceMapItem> itemsToRemove = new();
        foreach (Guid item in e.Items)
        {
            itemsToRemove.Add(_items[item]);
        }

        return new TimeSpaceRemoveItemsEvent
        {
            ItemsToRemove = itemsToRemove
        };
    }
}