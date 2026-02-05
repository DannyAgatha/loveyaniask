namespace WingsEmu.Game.Entities;

public interface IAiEntity : IBattleEntity
{
    public bool IsStillAlive { get; set; }
}