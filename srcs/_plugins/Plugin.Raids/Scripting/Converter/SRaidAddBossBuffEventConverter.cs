using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Scripting.Converter;

public class SRaidAddBossBuffEventConverter : ScriptedEventConverter<SRaidAddBossBuffEvent>
{
    private readonly IMapInstance instance;

    public SRaidAddBossBuffEventConverter(IMapInstance instance) => this.instance = instance;

    protected override IAsyncEvent Convert(SRaidAddBossBuffEvent e) => new RaidAddBossBuffEvent(e.Buff, instance);
}