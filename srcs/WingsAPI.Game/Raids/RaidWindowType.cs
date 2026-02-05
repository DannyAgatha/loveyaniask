namespace WingsEmu.Game.Raids;

public enum RaidWindowType : byte
{
    MISSION_START = 0,
    MISSION_CLEAR = 1,
    TIMES_UP = 2,
    LEADER_DEATH = 3,
    NO_LIVES_LEFT = 4,
    MISSION_FAIL = 5 // Used in Rainbow Battle
}