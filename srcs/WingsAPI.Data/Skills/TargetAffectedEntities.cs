namespace WingsEmu.DTOs.Skills;

public enum TargetAffectedEntities
{
    Enemies = 0,
    DebuffForEnemies = 1,
    BuffForAllies = 2,
    None = 3 // don't use su, cancel needs to be sent and uses some guri
}