// NosEmu
// 


namespace WingsEmu.Packets.Enums
{
    public enum QuestType : byte
    {
        KILL_MONSTER_BY_VNUM = 1,
        DROP_HARDCODED = 2,
        DROP_CHANCE = 3,
        DELIVER_ITEM_TO_NPC = 4,
        CAPTURE_WITHOUT_KEEPING = 5,
        CAPTURE_AND_KEEP = 6,
        COMPLETE_TIMESPACE = 7,
        CRAFT_WITHOUT_KEEPING = 8,
        DIE_X_TIMES = 9,
        EARN_REPUTATION = 10,
        COMPLETE_TIMESPACE_WITH_ATLEAST_X_POINTS = 11,
        DIALOG = 12,
        DROP_IN_TIMESPACE = 13,
        GIVE_ITEM_TO_NPC = 14,
        DIALOG_WHILE_WEARING = 15,
        DIALOG_WHILE_HAVING_ITEM = 16,
        DROP_CHANCE_2 = 17,
        GIVE_NPC_GOLD = 18,
        GO_TO_MAP = 19,
        COLLECT = 20,
        USE_ITEM_ON_TARGET = 21,
        DIALOG_2 = 22, // "Get more information"
        NOTHING = 23, // This quest will be finished automatically
        GIVE_ITEM_TO_NPC_2 = 24, // "Inspect"
        WIN_RAID_AND_TALK_TO_NPC = 25,
        KILL_X_MOBS_SOUND_FLOWER = 26,
        KILL_PLAYER_IN_REGION = 27
    }
}