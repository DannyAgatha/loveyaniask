using PhoenixLib.Events;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Raids.Events;

public class RaidRemoveBossBuffEvent : IAsyncEvent
{
    public RaidRemoveBossBuffEvent(short buffId, IMapInstance raidInstance)
    {
        BuffId = buffId;
        MapInstance = raidInstance;
    }

    public short BuffId { get; }

    public IMapInstance MapInstance { get; }
}