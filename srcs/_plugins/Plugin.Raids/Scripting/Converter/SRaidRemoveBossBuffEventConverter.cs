using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Scripting.Converter;

public class SRaidRemoveBossBuffEventConverter : ScriptedEventConverter<SRaidRemoveBossBuffEvent>
{
    private readonly IMapInstance instance;

    public SRaidRemoveBossBuffEventConverter(IMapInstance instance) => this.instance = instance;

    protected override IAsyncEvent Convert(SRaidRemoveBossBuffEvent e) => new RaidRemoveBossBuffEvent(e.Buff, instance);
}