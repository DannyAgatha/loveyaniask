namespace WingsEmu.Packets.ServerPackets.Battle
{
    public enum SuPacketHitMode : sbyte
    {
        NoDamageFail = -2,
        NoDamageSuccess = -1,
        SuccessAttack = 0,
        Miss = 1,
        OutOfRange = 2,
        CriticalAttack = 3,
        MissAoe = 4,
        AttackedInAoe = 5,
        AttackedInAoeCrit = 6,
        ReflectionAoeMiss = 7
    }
}