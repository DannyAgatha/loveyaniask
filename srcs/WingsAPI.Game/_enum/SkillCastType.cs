namespace WingsEmu.Game._enum;

public enum SkillCastType : byte
{
    BEFORE_ATTACK_SELF = 0,
    BEFORE_ATTACK_ON_MAIN_TARGET = 1,
    AFTER_ATTACK_ALL_ALLIES = 2,
    AFTER_ATTACK_ALL_TARGETS = 3,
    BEFORE_ATTACK_ALL_TARGETS = 4
}