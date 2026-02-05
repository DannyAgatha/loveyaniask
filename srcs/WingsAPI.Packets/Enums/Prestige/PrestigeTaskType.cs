namespace WingsAPI.Packets.Enums.Prestige;

public enum PrestigeTaskType
{
    KILL_MONSTERS_BY_LEVEL,        // Defeat monsters equal to or above your level
    COMPLETE_RAID,                 // Complete a raid
    WIN_PVP_IN_MAP,                // Win PvP matches in a specific map
    COLLECT_GOLD,                  // Collect a certain amount of gold
    SPEND_GOLD_NPC,                // Spend gold at NPCs
    GAIN_FAME,                     // Gain fame points
    REACH_LEVEL,                   // Reach a required base level
    REACH_HERO_LEVEL,              // Reach a required hero level
    COLLECT_ITEM,                  // Collect a specific item
    COMPLETE_ELEMENTAL_RAID,       // Complete an elemental raid
    KILL_MONSTERS_BY_VNUM,          // Defeat monsters with a specific VNUM
    KILL_MONSTER_BOSS_BY_VNUM       // Defeat a specific boss monster by VNUM
}