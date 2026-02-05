namespace WingsEmu.Game.Entities.Event;

public class MapLeaveMonsterEntityEvent : MonsterEntityEvent
{
    public MapLeaveMonsterEntityEvent(IMonsterEntity monsterEntity) : base(monsterEntity)
    {
    }
}