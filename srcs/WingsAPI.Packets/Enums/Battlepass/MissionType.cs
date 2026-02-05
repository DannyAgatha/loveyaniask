namespace WingsAPI.Packets.Enums.BattlePass;

public enum MissionType : byte
{
    ListOfFamilySkill, // ??? Unused none info
    Commend,
    CompleteXRaids,
    CompleteXDailyQuest,
    DefeatXMonsterInRange,
    CompleteXRainbowBattle,
    CompleteXInstantCombat,
    CompleteXArenaOfTalent,
    PlayXMinilandGame,
    CatchXFish,
    CompleteXTimeSpaceLvl30Plus,
    DefeatXCurserMob,
    CompleteXArenaOfMaster,
    CookXMeal,
    CompleteXMinigameRaid,
    CompleteXCaligor,
    UpgradeSpCardOrGearXTime,
    CraftXItem,
    KillXPlayerInGlacernon,
    ReachXReputation,
    StayLoggedXMinute,
    DefeatXBossMap,
    ReachXLevelOnCelestialSpire,
    LoginXTimeInARow,
    SpendXGoldToNpc,
    CompleteXTimeSpaceExcHidden,
    CompleteXHiddenTimeSpace,
    CompleteXLevelOnCelestialSpire,
    Earn2000PointInCombatArenaInXMinute,
    CompleteRaidXTime, // FirstData = RaidType, Time = max
    DefeatXMonsterVnum,// FirstData = vnum, Time = max
    CraftXVnumItem, // Max,FirstData
    DefeatXRaceTypeMonsterTime, // FirstData = race
    PlayMiniGameLevel, // FirstData = vnum, lvl = max
    HarvestXItemVnum, // FirstData = vnum, lvl = max
    SuccessRaidInXTime, // FirstData = RaiType, sec = max
    StarPetTrainer,// FirstData = Amount, Star = max
    LevelPetTrainer, // FirstData = Amount, Lvl = max
    CapturePetTrainer, // FirstData = Amount, star = max
    LevelPetTrainerVNumMobXlevel, // FirstData = MobVnum, lvl = max
}