using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Miniland.Events;

public class AddObjMinilandEndLogicEvent : PlayerEvent
{
    public AddObjMinilandEndLogicEvent(MapDesignObject mapObject, IMapInstance miniland)
    {
        MapObject = mapObject;
        Miniland = miniland;
    }

    public MapDesignObject MapObject { get; }
    public IMapInstance Miniland { get; }
}

public class MinilandChestViewContentEvent : PlayerEvent
{
    public MinilandChestViewContentEvent(int chestVnum) => ChestVnum = chestVnum;

    public int ChestVnum { get; }
}