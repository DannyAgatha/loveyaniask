namespace WingsEmu.Game.Entities.Event;

public class GenerateEntityDeathEvent : IBattleEntityEvent
{
    public IBattleEntity Attacker { get; init; }

    public bool? IsByMainWeapon { get; init; }
    public IBattleEntity Entity { get; init; }
}