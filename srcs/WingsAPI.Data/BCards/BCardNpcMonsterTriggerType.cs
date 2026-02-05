namespace WingsEmu.DTOs.BCards;

public enum BCardNpcMonsterTriggerType
{
    NONE = 0, // should always trigger
    ON_FIRST_ATTACK = 1, // triggers only on first hit
    ON_DEATH = 2 // triggers on npcMonster's death
}