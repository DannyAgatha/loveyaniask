// NosEmu
// 


namespace WingsEmu.Game.Battle;

public enum HostilityType
{
    NOT_HOSTILE,
    ATTACK_IN_RANGE = 1,
    ATTACK_MATES = 4, // pets
    ATTACK_DEVILS_ONLY = 23,
    ATTACK_ANGELS_ONLY = 24,

    ATTACK_NOT_WEARING_PHANTOM_AMULET = 100
    // if > 20 000, attacks the people who have HostilityType - 20000 quest
}