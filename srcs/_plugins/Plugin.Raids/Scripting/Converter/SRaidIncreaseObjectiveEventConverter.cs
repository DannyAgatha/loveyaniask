using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Raid;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Scripting.Converter;

public class SRaidIncreaseObjectiveEventConverter : ScriptedEventConverter<SRaidIncreaseObjectiveEvent>
{
    private readonly RaidSubInstance instance;

    public SRaidIncreaseObjectiveEventConverter(RaidSubInstance instance) => this.instance = instance;

    protected override IAsyncEvent Convert(SRaidIncreaseObjectiveEvent e) => new RaidObjectiveIncreaseEvent((RaidTargetType)(byte)e.ObjectiveType, instance);
}