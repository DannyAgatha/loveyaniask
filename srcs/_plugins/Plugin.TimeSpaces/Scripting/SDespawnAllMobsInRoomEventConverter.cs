using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class SDespawnAllMobsInRoomEventConverter : ScriptedEventConverter<SDespawnAllMobsInRoomEvent>
{
    private readonly Dictionary<Guid, TimeSpaceSubInstance> _maps;

    public SDespawnAllMobsInRoomEventConverter(Dictionary<Guid, TimeSpaceSubInstance> maps) => _maps = maps;

    protected override IAsyncEvent Convert(SDespawnAllMobsInRoomEvent e) =>
        new TimeSpaceDespawnMonstersInRoomEvent
        {
            TimeSpaceSubInstance = _maps[e.Map]
        };
}