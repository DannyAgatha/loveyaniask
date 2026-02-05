using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameDurabilityInfoEvent : PlayerEvent
{
    public MinigameDurabilityInfoEvent(MapDesignObject mapObject) => MapObject = mapObject;

    public MapDesignObject MapObject { get; }
}