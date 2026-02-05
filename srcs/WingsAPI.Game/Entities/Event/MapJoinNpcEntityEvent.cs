namespace WingsEmu.Game.Entities.Event;

public class MapJoinNpcEntityEvent : NpcEntityEvent
{
    public MapJoinNpcEntityEvent(INpcEntity npcEntity, short? mapX = null, short? mapY = null) : base(npcEntity)
    {
        MapX = mapX;
        MapY = mapY;
    }

    public short? MapX { get; }
    public short? MapY { get; }
}