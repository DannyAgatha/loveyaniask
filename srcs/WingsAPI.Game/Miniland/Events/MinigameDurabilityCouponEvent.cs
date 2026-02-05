using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameDurabilityCouponEvent : PlayerEvent
{
    public MinigameDurabilityCouponEvent(MapDesignObject mapObject) => MapObject = mapObject;

    public MapDesignObject MapObject { get; }
}