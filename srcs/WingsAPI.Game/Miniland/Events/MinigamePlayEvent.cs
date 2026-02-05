using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigamePlayEvent : PlayerEvent
{
    public MinigamePlayEvent(MapDesignObject minigameObject, bool isForFun)
    {
        MinigameObject = minigameObject;
        IsForFun = isForFun;
    }

    public bool IsForFun { get; }

    public MapDesignObject MinigameObject { get; }
}