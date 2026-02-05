using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WingsAPI.Data.BattlePass;
using WingsAPI.Data.Character;
using WingsAPI.Data.Miniland;
using WingsAPI.Data.Prestige;
using WingsAPI.Data.WorldBoss;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Mates;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Cheats;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Families;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Hardcore;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quicklist;
using WingsEmu.Game.Raids;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Relations;
using WingsEmu.Game.RespawnReturn;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Skills;
using WingsEmu.Game.SnackFood;
using WingsEmu.Game.Specialists;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Characters;

public interface IPlayerEntity : IBattleEntity, IEquipmentOptionContainer, IQuestContainer, ICharacterRevivalComponent, IFamilyComponent, IComboSkillComponent, ISkillCooldownComponent,
    IAngelElementBuffComponent, IScoutComponent, IRelationComponent, IGroupComponent, IInventoryComponent, IExchangeComponent, IBubbleComponent, IPartnerInventoryComponent,
    IRaidComponent, IFoodSnackComponent
{
    bool HasUpgradedMartialArtist { get; set; }
    bool HasUnlockedDesiredBearing(IReadOnlyList<Recipe> recipes, byte bearing);
    int FoodValue { get; set; }
    DateTime LastFoodProcess { get; set; }
    List<CharacterCookingDto> CharacterCookingDto { get; set; }
    bool IsCooking { get; set; }
    bool CanCollectMeal { get; set; }
    IDisposable FirstChefAction { get; set; }
    IDisposable SecondChefAction { get; set; }
    IDisposable ThirdChefAction { get; set; }
    Recipe LastRecipeFromChefSp { get; set; }
    int LastSkillId { get; set; }
    bool HaveBpUiOpen { get; set; }
    bool HasCaughtFish { get; set; }
    bool HasFishingLineBroke{ get; set; }
    bool HasBadLuck{ get; set; }
    IDisposable  FishingFirstFish { get; set; }
    IDisposable  FishingSecondFish { get; set; }
    DateTime BattlePassProcess { get; set; }
    short FishInteraction { get; set; }
    Tuple<short, short, short> FishSavedLocation { get; set; }
    bool CanCollectFish { get; set; }
    bool IsRareFish { get; set; }
    byte UsageSkillWithoutCd { get; set; }
    bool WasMorphedPreviously { get; set; }
    bool BlockAllAttack { get; set; }
    DateTime LastSlotChange { get; set; }
    int UntransformPressingKeyCount { get; set; }
    int AdditionalHp { get; set; }
    int AdditionalMp { get; set; }
    bool TriggerAmbush { get; set; }
    DateTime LastMapChange { get; set; }
    DateTime? LastSkillCombo { get; set; }
    AuthorityType Authority { get; set; }
    int CurrentMinigame { get; set; }
    int SecondDamageMinimum { get; }
    int SecondDamageMaximum { get; }
    int HitRate { get; }
    int HitCriticalChance { get; }
    int HitCriticalDamage { get; }
    int SecondHitRate { get; }
    int SecondHitCriticalChance { get; }
    int SecondHitCriticalDamage { get; }
    int MeleeDefence { get; }
    int RangedDefence { get; }
    int MagicDefence { get; }
    int MeleeDodge { get; }
    int RangedDodge { get; }
    int SpecialistElementRate { get; }
    DateTime GameStartDate { get; set; }
    bool HasShopOpened { get; set; }
    bool Invisible { get; }
    bool IsCustomSpeed { get; set; }
    IAlzanorComponent AlzanorComponent { get; }
    bool IsShopping { get; set; }
    bool IsSitting { get; set; }
    bool IsOnVehicle { get; set; }
    bool IsMorphed { get; set; }
    (VisualType, long) LastEntity { get; set; }
    LastWalk LastWalk { get; set; }
    int LastNRunId { get; set; }
    int LastPulse { get; set; }
    DateTime LastPulseTick { get; set; }
    DateTime LastDefence { get; set; }
    DateTime LastItemUpgrade { get; set; }
    DateTime LastDeath { get; set; }
    DateTime LastEffect { get; set; }
    DateTime LastEffectMinigame { get; set; }
    DateTime LastHealth { get; set; }
    DateTime LastPortal { get; set; }
    DateTime LastPotion { get; set; }
    DateTime LastSnack { get; set; }
    DateTime LastFood { get; set; }
    DateTime LastSkillUse { get; set; }
    DateTime LastSpeedChange { get; set; }
    DateTime LastTransform { get; set; }
    DateTime LastEnergy { get; set; }
    DateTime LastEnergyRefill { get; set; }
    DateTime LastTokenEnergy { get; set; }
    DateTime LastTokenEnergyRefill { get; set; }
    DateTime? SpCooldownEnd { get; set; }
    DateTime Bubble { get; set; }
    DateTime SpyOutStart { get; set; }
    DateTime ItemsToRemove { get; set; }
    DateTime BonusesToRemove { get; set; }
    DateTime? RandomMapTeleport { get; set; }
    DateTime LastMove { get; set; }
    DateTime LastPutItem { get; set; }
    DateTime LastSentNote { get; set; }
    DateTime? CheckWeedingBuff { get; set; }
    DateTime LastPvPAttack { get; set; }
    DateTime LastRainbowArrowEffect { get; set; }
    DateTime LastRainbowEffects { get; set; }
    DateTime LastIcebreakerEffects { get; set; }
    DateTime LastAct6PvpEffects { get; set; }
    DateTime LastTimeDisplayed { get; set; }
    DateTime LastSunWolfTeleport { get; set; }
    Guid MapInstanceId { get; set; }
    IMapInstance Miniland { get; set; }
    int Morph { get; set; }
    int? PreviousMorph { get; set; }
    int MorphUpgrade { get; set; }
    int MorphUpgrade2 { get; set; }
    IClientSession Session { get; }
    ConcurrentDictionary<int, CharacterSkill> CharacterSkills { get; }
    ConcurrentDictionary<int, CharacterSkill> SkillsSp { get; set; }
    ConcurrentDictionary<long, int> HitsByMonsters { get; }
    bool UseSp { get; set; }
    byte VehicleSpeed { get; set; }
    byte VehicleMapSpeed { get; set; }
    int WareHouseSize { get; set; }
    DateTime LastBuySearchBazaarRefresh { get; set; }
    DateTime LastBuyBazaarRefresh { get; set; }
    DateTime LastListItemBazaar { get; set; }
    DateTime LastAdministrationBazaarRefresh { get; set; }
    DateTime LastMonsterCaught { get; set; }
    bool IsSeal { get; set; }
    bool IsRemovingSpecialistPoints { get; set; }
    bool IsWarehouseOpen { get; set; }
    bool IsPartnerWarehouseOpen { get; set; }
    bool IsCraftingItem { get; set; }
    bool IsBankOpen { get; set; }
    bool IsSunWolfDead { get; set; }
    DateTime LastUnfreezedPlayer { get; set; }
    DateTime LastUpgradePet { get; set; }
    DateTime LastSpPacketSent { get; set; }
    DateTime LastSpRemovingProcess { get; set; }
    DateTime LastAttack { get; set; }
    bool InitialScpPacketSent { get; set; }
    long AccountId { get; set; }
    int Act4Dead { get; set; }
    int Act4Kill { get; set; }
    int Act4Points { get; set; }
    int ArenaWinner { get; set; }
    string Biography { get; set; }
    bool BuffBlocked { get; set; }
    bool ShowRaidDeathInfo { get; set; }
    ClassType Class { get; set; }
    SubClassType SubClass { get; set; }
    short Compliment { get; set; }
    float Dignity { get; set; }
    bool EmoticonsBlocked { get; set; }
    bool ExchangeBlocked { get; set; }
    bool FamilyRequestBlocked { get; set; }
    bool FriendRequestBlocked { get; set; }
    GenderType Gender { get; set; }
    long Gold { get; set; }
    bool GroupRequestBlocked { get; set; }
    HairColorType HairColor { get; set; }
    HairStyleType HairStyle { get; set; }
    bool HeroChatBlocked { get; set; }
    byte HeroLevel { get; set; }
    long HeroXp { get; set; }
    bool HpBlocked { get; set; }
    bool IsPetAutoRelive { get; set; }
    bool IsPartnerAutoRelive { get; set; }
    byte JobLevel { get; set; }
    long JobLevelXp { get; set; }
    long LevelXp { get; set; }
    int MapId { get; set; }
    short MapX { get; set; }
    short MapY { get; set; }
    int MasterPoints { get; set; }
    int MasterTicket { get; set; }
    byte MaxPetCount { get; set; }
    byte MaxPartnerCount { get; set; }
    bool MinilandInviteBlocked { get; set; }
    string MinilandMessage { get; set; }
    short MinilandPoint { get; set; }
    MinilandState MinilandState { get; set; }
    bool MouseAimLock { get; set; }
    string Name { get; set; }
    bool QuickGetUp { get; set; }
    bool HideHat { get; set; }
    bool HideCD { get; set; }
    bool HideHPMP { get; set; }
    bool UiBlocked { get; set; }
    long Reput { get; set; }
    byte Slot { get; set; }
    int SpPointsBonus { get; set; }
    int SpPointsBasic { get; set; }
    int EnergyBar { get; set; }
    int SecondEnergyBar { get; set; }
    int EnergyRemoved { get; set; }
    int TokenEnergyBar { get; set; }
    int TokenGauge { get; set; }
    int TokenEnergyBarRemoved { get; set; }
    int TokenGaugeRemoved { get; set; }
    int TalentLose { get; set; }
    int TalentSurrender { get; set; }
    int TalentWin { get; set; }
    bool WhisperBlocked { get; set; }
    int? LastMinilandProducedItem { get; set; }
    bool IsGettingLosingReputation { get; set; }
    byte DeathsOnAct4 { get; set; }
    long ArenaKills { get; set; }
    long ArenaDeaths { get; set; }
    byte TierLevel { get; set; }
    long TierExperience { get; set; }
    TimeSpan? MuteRemainingTime { get; set; }
    DateTime LastMuteTick { get; set; }
    DateTime LastSitting { get; set; }
    DateTime? LastChatMuteMessage { get; set; }
    DateTime LastInventorySort { get; set; }
    DateTime? ArenaImmunity { get; set; }
    int DamageInRaid { get; set; }
    long InstantCombatDamage { get; set; }
    byte KillStreak { get; set; }
    bool IsInKillStreak { get; set; }
    bool HasNosBazaarOpen { get; set; }
    bool PreventEnergyRemove { get; set; }
    bool HasAutoLootEnabled { get; set; }
    bool CancelUpgrade { get; set; }
    List<Guid> SignPostMapInstanceIds { get; }
    List<CharacterPartnerInventoryItemDto> PartnerInventory { get; set; }
    List<MateDTO> NosMates { get; set; }
    List<long> CompletedTimeSpaces { get; set; }
    List<PartnerWarehouseItemDto> PartnerWarehouse { get; set; }
    List<CharacterStaticBonusDto> Bonus { get; set; }
    List<CharacterStaticBuffDto> StaticBuffs { get; set; }
    List<CharacterQuicklistEntryDto> Quicklist { get; set; }
    List<CharacterSkillDTO> LearnedSkills { get; set; }
    List<CharacterTitleDto> Titles { get; set; }
    List<CompletedScriptsDto> CompletedScripts { get; set; }
    List<CharacterQuestDto> CompletedQuests { get; set; }
    List<CharacterQuestDto> CompletedPeriodicQuests { get; set; }
    List<CharacterQuestDto> ActiveQuests { get; set; }
    List<CharacterMinilandObjectDto> MinilandObjects { get; set; }
    List<CharacterInventoryItemDto> Inventory { get; set; }
    List<CharacterInventoryItemDto> EquippedStuffs { get; set; }
    CharacterLifetimeStatsDto LifetimeStats { get; set; }
    CharacterRaidRestrictionDto RaidRestrictionDto { get; set; }
    RainbowBattleLeaverBusterDto RainbowBattleLeaverBusterDto { get; set; }
    IcebreakerLeaverBusterDto IcebreakerLeaverBusterDto { get; set; }
    CharacterTrophyFragmentsDto TrophyFragmentsDto { get; set; }
    List<BattlePassItemDto> BattlePassItemDto { get; set; }
    List<BattlePassQuestDto> BattlePassQuestDto { get; set; }
    List<CharacterPityDto> PityDto { get; set; }
    List<CharacterFishDto> FishDto { get; set; }
    List<CharacterFishRewardsEarnedDto> FishRewardsEarnedDto { get; set; }
    BattlePassOptionDto BattlePassOptionDto { get; set; }
    List<MaxStarTrainerDto> MaxStarTrainerDto { get; set; }
    List<TrainerQuestDto> TrainerQuestDto { get; set; }
    CharacterItemRestrictionDto ItemRestrictionDto { get; set; }
    List<DailyRewardDto> DailyRewardDto { get; set; }
    LandOfLifeRestrictionDto LandOfLifeRestrictionDto { get; set; }
    CharacterPrestigeDto CharacterPrestigeDto { get; set; }
    List<WorldBossRecordDto> WorldBossRecordsDto { get; set; }
    InventoryItem LastSeal { get; set; }
    DateTime LastRaidCreate { get; set; }
    bool IsAvailableToChangeName { get; set; }
    IQuicklistComponent QuicklistComponent { get; }
    IMateComponent MateComponent { get; }
    IHomeComponent HomeComponent { get; }
    ISkillComponent SkillComponent { get; }
    ICheatComponent CheatComponent { get; }
    ISpecialistStatsComponent SpecialistComponent { get; }
    IPlayerStatisticsComponent StatisticsComponent { get; }
    ITimeSpaceComponent TimeSpaceComponent { get; }
    IShopComponent ShopComponent { get; }
    IMailNoteComponent MailNoteComponent { get; }
    IRainbowBattleComponent RainbowBattleComponent { get; }
    IHardcoreComponent HardcoreComponent { get; }
    PrivateMapInstanceInfo PrivateMapInstanceInfo { get; set; }
    Guid? UnderWaterShowdownId { get; set; }
    bool IsDraconicMorphed { get; set; }
    DateTime LastDraconicMorph { get; set; }
    bool IsFlameDruidTransformed { get; set; }
    DateTime LastFlameDruid { get; set; }
    void RefreshCharacterStats(bool refreshHpMp = true);
    void AddStaticBonus(CharacterStaticBonusDto bonus);
    void AddStaticBonuses(IEnumerable<CharacterStaticBonusDto> bonuses);
    IReadOnlyList<CharacterStaticBonusDto> GetStaticBonuses();
    CharacterStaticBonusDto GetStaticBonus(Predicate<CharacterStaticBonusDto> predicate);
    int GetCp();
    int GetDignityIco();
    bool IsQuestActive(int questId);
    List<IPortalEntity> GetExtraPortal();
    void SetSession(IClientSession clientSession);
    int HealthHpLoad();
    int HealthMpLoad();
    bool HasBuff(BuffVnums buffVnum);
    bool HasBuff(short buffVnum);
    void SetFaction(FactionType faction);
    FactionType? CaligorFaction { get; set; }
}