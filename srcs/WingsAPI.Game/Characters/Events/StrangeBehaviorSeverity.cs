namespace WingsEmu.Game.Characters.Events;

public enum StrangeBehaviorSeverity
{
    NORMAL, // harmless packet logger usage
    ABUSING, // harmful packet logger usage
    SEVERE_ABUSE, // trying to exploit without high game severity (teleporting...)
    DANGER // trying to crash, dupe...
}