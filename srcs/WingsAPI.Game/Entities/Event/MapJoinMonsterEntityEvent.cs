namespace WingsEmu.Game.Entities.Event;

public class MapJoinMonsterEntityEvent : MonsterEntityEvent
{
    public MapJoinMonsterEntityEvent(IMonsterEntity monsterEntity, short? mapX = null, short? mapY = null, bool showEffect = false) : base(monsterEntity)
    {
        MapX = mapX;
        MapY = mapY;
        ShowEffect = showEffect;
    }

    public short? MapX { get; }
    public short? MapY { get; }
    public bool ShowEffect { get; }
}