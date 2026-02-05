using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Raid;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Scripting.Converter;

public class SFinishRaidEventConverter : ScriptedEventConverter<SFinishRaidEvent>
{
    private readonly RaidParty raidParty;

    public SFinishRaidEventConverter(RaidParty raidParty) => this.raidParty = raidParty;

    protected override IAsyncEvent Convert(SFinishRaidEvent e) => new RaidInstanceFinishEvent(raidParty, (RaidFinishType)(byte)e.FinishType);
}