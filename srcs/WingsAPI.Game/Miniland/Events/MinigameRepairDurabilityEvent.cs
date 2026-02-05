using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameRepairDurabilityEvent : PlayerEvent
{
    public MinigameRepairDurabilityEvent(MapDesignObject mapObject, long goldToExpend)
    {
        MapObject = mapObject;
        GoldToExpend = goldToExpend;
    }

    public MapDesignObject MapObject { get; }

    public long GoldToExpend { get; }
}