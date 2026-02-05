namespace WingsEmu.Game.Entities;

public class MonsterEntityEvent : IBattleEntityEvent
{
    public MonsterEntityEvent(IMonsterEntity monsterEntity) => MonsterEntity = monsterEntity;

    public IMonsterEntity MonsterEntity { get; }
    public IBattleEntity Entity => MonsterEntity;
}