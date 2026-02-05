using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameScoreEvent : PlayerEvent
{
    public MinigameScoreEvent(MapDesignObject mapObject, long score1, long score2)
    {
        MapObject = mapObject;
        Score1 = score1;
        Score2 = score2;
    }

    public MapDesignObject MapObject { get; }

    public long Score1 { get; }

    public long Score2 { get; }
}