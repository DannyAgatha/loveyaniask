using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class SCheckForTasksCompletedEventConverter : ScriptedEventConverter<SCheckForTasksCompletedEvent>
{
    private readonly Dictionary<Type, IScriptedEventConverter> _events;
    private readonly Dictionary<Guid, TimeSpaceSubInstance> _maps;

    public SCheckForTasksCompletedEventConverter(Dictionary<Guid, TimeSpaceSubInstance> maps, Dictionary<Type, IScriptedEventConverter> events)
    {
        _maps = maps;
        _events = events;
    }

    protected override IAsyncEvent Convert(SCheckForTasksCompletedEvent e)
    {
        List<IAsyncEvent> list = new();
        List<TimeSpaceSubInstance> timeSpaceSubInstances = new();
        IEnumerable<Guid> maps = e.Maps;

        foreach (SEvent scriptedEvent in e.Events)
        {
            IAsyncEvent asyncEvent = _events.GetValueOrDefault(scriptedEvent.GetType())?.Convert(scriptedEvent);
            if (asyncEvent == null)
            {
                continue;
            }

            list.Add(asyncEvent);
        }

        foreach (Guid map in maps)
        {
            timeSpaceSubInstances.Add(_maps[map]);
        }

        return new TimeSpaceCheckForTasksCompletedEvent
        {
            Events = list,
            TimeSpaceSubInstances = timeSpaceSubInstances
        };
    }
}