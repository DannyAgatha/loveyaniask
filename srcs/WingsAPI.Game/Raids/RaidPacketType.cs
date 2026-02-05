namespace WingsEmu.Game.Raids;

public enum RaidPacketType
{
    LIST_MEMBERS = 0,
    LEAVE = 1,
    LEADER_RELATED = 2,
    REFRESH_MEMBERS_HP_MP = 3,
    AFTER_INSTANCE_START_BUT_BEFORE_REFRESH_MEMBERS = 4,
    INSTANCE_START = 5
}