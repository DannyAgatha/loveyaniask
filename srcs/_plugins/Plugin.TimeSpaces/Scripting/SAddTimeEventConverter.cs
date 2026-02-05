using System;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class SAddTimeEventConverter : ScriptedEventConverter<SAddTimeEvent>
{
    private readonly TimeSpaceParty _timeSpaceParty;

    public SAddTimeEventConverter(TimeSpaceParty timeSpaceParty) => _timeSpaceParty = timeSpaceParty;

    protected override IAsyncEvent Convert(SAddTimeEvent e) =>
        new TimeSpaceAddTimeToTimerEvent
        {
            TimeSpaceParty = _timeSpaceParty,
            Time = TimeSpan.FromSeconds(e.Time)
        };
}