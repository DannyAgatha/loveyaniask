// NosEmu
// 


namespace WingsEmu.Packets.Enums.Battle
{
    public enum AttackType : byte
    {
        Melee = 0,
        Ranged = 1,
        Magical = 2,
        Other = 3,
        Charge = 4,
        Dash = 5 // all skills that changes player position
    }
}