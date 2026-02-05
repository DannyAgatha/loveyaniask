using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameStopEvent : PlayerEvent
{
    public MinigameStopEvent(MapDesignObject minigameObject) => MinigameObject = minigameObject;

    public MapDesignObject MinigameObject { get; }
}