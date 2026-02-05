using System;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class ScriptSetTimeEventConverter : ScriptedEventConverter<ScriptSetTimeEvent>
{
    private readonly TimeSpaceParty _timeSpaceParty;

    public ScriptSetTimeEventConverter(TimeSpaceParty timeSpaceParty) => _timeSpaceParty = timeSpaceParty;

    protected override IAsyncEvent Convert(ScriptSetTimeEvent e) =>
        new TimeSpaceSetTimeEvent
        {
            TimeSpaceParty = _timeSpaceParty,
            Time = TimeSpan.FromSeconds(e.Time)
        };
}