using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameGetYieldInfoEvent : PlayerEvent
{
    public MinigameGetYieldInfoEvent(MapDesignObject mapObject) => MapObject = mapObject;

    public MapDesignObject MapObject { get; }
}