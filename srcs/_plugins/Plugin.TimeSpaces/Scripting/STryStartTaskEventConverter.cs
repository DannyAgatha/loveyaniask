using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class STryStartTaskEventConverter : ScriptedEventConverter<STryStartTaskEvent>
{
    private readonly Dictionary<Guid, TimeSpaceSubInstance> _maps;

    public STryStartTaskEventConverter(Dictionary<Guid, TimeSpaceSubInstance> maps) => _maps = maps;

    protected override IAsyncEvent Convert(STryStartTaskEvent e) =>
        new TryStartTaskEvent
        {
            Map = _maps[e.MapId]
        };
}