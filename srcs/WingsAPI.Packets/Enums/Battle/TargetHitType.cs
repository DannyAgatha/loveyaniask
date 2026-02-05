// NosEmu
// 


namespace WingsEmu.Packets.Enums.Battle
{
    public enum TargetHitType : byte
    {
        TargetOnly = 0,
        EnemiesInAffectedAoE = 1,
        AlliesInAffectedAoE = 2,
        SpecialArea = 3,
        PlayerAndHisMates = 4,
        SpecialZoneHit = 5
    }
}