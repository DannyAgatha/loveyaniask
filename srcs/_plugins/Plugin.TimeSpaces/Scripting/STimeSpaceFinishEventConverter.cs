using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class STimeSpaceFinishEventConverter : ScriptedEventConverter<STimeSpaceFinishEvent>
{
    private readonly TimeSpaceParty _timeSpaceParty;

    public STimeSpaceFinishEventConverter(TimeSpaceParty timeSpaceParty) => _timeSpaceParty = timeSpaceParty;

    protected override IAsyncEvent Convert(STimeSpaceFinishEvent e) => new TimeSpaceInstanceFinishEvent(_timeSpaceParty, (TimeSpaceFinishType)e.TimeSpaceFinishType);
}