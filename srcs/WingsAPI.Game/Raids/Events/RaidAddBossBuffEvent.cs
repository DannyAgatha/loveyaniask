using PhoenixLib.Events;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Raids.Events;

public class RaidAddBossBuffEvent : IAsyncEvent
{
    public RaidAddBossBuffEvent(short buffId, IMapInstance mapInstance)
    {
        BuffId = buffId;
        MapInstance = mapInstance;
    }

    public short BuffId { get; }

    public IMapInstance MapInstance { get; }
}