namespace WingsEmu.Game.Entities.Event;

public class MapLeaveNpcEntityEvent : NpcEntityEvent
{
    public MapLeaveNpcEntityEvent(INpcEntity npcEntity) : base(npcEntity)
    {
    }
}