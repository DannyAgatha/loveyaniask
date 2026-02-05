namespace WingsEmu.Game.Entities;

public class NpcEntityEvent : IBattleEntityEvent
{
    public NpcEntityEvent(INpcEntity npcEntity) => NpcEntity = npcEntity;

    public INpcEntity NpcEntity { get; }
    public IBattleEntity Entity => NpcEntity;
}