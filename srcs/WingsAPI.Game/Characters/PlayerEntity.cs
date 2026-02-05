// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.BattlePass;
using WingsAPI.Data.Character;
using WingsAPI.Data.Miniland;
using WingsAPI.Data.Prestige;
using WingsAPI.Data.WorldBoss;
using WingsEmu.Core.Generics;
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
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Cheats;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Hardcore;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
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
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Characters;

public partial class PlayerEntity : IPlayerEntity
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IFamilyManager _familyManager;
    private readonly IFoodSnackComponentFactory _foodSnackComponentFactory;
    private readonly IMapManager _mapManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;
    private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;

    private int? _size;
    private long _gold;

    public PlayerEntity(CharacterDTO characterDto, IFamilyManager familyManager, IRandomGenerator randomGenerator, IMapManager mapManager, ICharacterAlgorithm characterAlgorithm,
        IFoodSnackComponentFactory foodSnackComponentFactory, IAsyncEventPipeline eventPipeline, IBattleEntityAlgorithmService algorithm, IPortalFactory portalFactory, IItemsManager itemsManager, IBuffFactory buffFactory, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
    {
        Id = (int)characterDto.Id;
        Hp = characterDto.Hp;
        Mp = characterDto.Mp;
        Level = characterDto.Level;
        Faction = characterDto.Faction;
        AccountId = characterDto.AccountId;
        Act4Dead = characterDto.Act4Dead;
        Act4Kill = characterDto.Act4Kill;
        Act4Points = characterDto.Act4Points;
        ArenaWinner = characterDto.ArenaWinner;
        Biography = characterDto.Biography;
        BuffBlocked = characterDto.BuffBlocked;
        Class = characterDto.Class;
        SubClass = characterDto.SubClass;
        TierLevel = characterDto.TierLevel;
        TierExperience = characterDto.TierExperience;
        Compliment = characterDto.Compliment;
        Dignity = characterDto.Dignity;
        EmoticonsBlocked = characterDto.EmoticonsBlocked;
        ExchangeBlocked = characterDto.ExchangeBlocked;
        FamilyRequestBlocked = characterDto.FamilyRequestBlocked;
        FriendRequestBlocked = characterDto.FriendRequestBlocked;
        Gender = characterDto.Gender;
        Gold = characterDto.Gold;
        GroupRequestBlocked = characterDto.GroupRequestBlocked;
        HairColor = characterDto.HairColor;
        HairStyle = characterDto.HairStyle;
        HeroChatBlocked = characterDto.HeroChatBlocked;
        HeroLevel = characterDto.HeroLevel;
        HeroXp = characterDto.HeroXp;
        HpBlocked = characterDto.HpBlocked;
        IsPetAutoRelive = characterDto.IsPetAutoRelive;
        IsPartnerAutoRelive = characterDto.IsPartnerAutoRelive;
        JobLevel = characterDto.JobLevel;
        JobLevelXp = characterDto.JobLevelXp;
        LevelXp = characterDto.LevelXp;
        MapId = characterDto.MapId;
        MapX = characterDto.MapX;
        MapY = characterDto.MapY;
        MasterPoints = characterDto.MasterPoints;
        MasterTicket = characterDto.MasterTicket;
        MaxPetCount = characterDto.MaxPetCount;
        MaxPartnerCount = characterDto.MaxPartnerCount;
        MinilandInviteBlocked = characterDto.MinilandInviteBlocked;
        MinilandMessage = characterDto.MinilandMessage;
        MinilandPoint = characterDto.MinilandPoint;
        MinilandState = characterDto.MinilandState;
        MouseAimLock = characterDto.MouseAimLock;
        Name = characterDto.Name;
        QuickGetUp = characterDto.QuickGetUp;
        HideHat = characterDto.HideHat;
        HideCD = characterDto.HideCD;
        HideHPMP = characterDto.HideHPMP;
        UiBlocked = characterDto.UiBlocked;
        Reput = characterDto.Reput;
        Slot = characterDto.Slot;
        SpPointsBonus = characterDto.SpPointsBonus;
        SpPointsBasic = characterDto.SpPointsBasic;
        TalentLose = characterDto.TalentLose;
        TalentSurrender = characterDto.TalentSurrender;
        TalentWin = characterDto.TalentWin;
        WhisperBlocked = characterDto.WhisperBlocked;
        PartnerInventory = characterDto.PartnerInventory;
        NosMates = characterDto.NosMates;
        PartnerWarehouse = characterDto.PartnerWarehouse;
        Bonus = characterDto.Bonus;
        StaticBuffs = characterDto.StaticBuffs;
        Quicklist = characterDto.Quicklist;
        LearnedSkills = characterDto.LearnedSkills;
        Titles = characterDto.Titles;
        CompletedScripts = characterDto.CompletedScripts;
        CompletedQuests = characterDto.CompletedQuests;
        CompletedPeriodicQuests = characterDto.CompletedPeriodicQuests;
        ActiveQuests = characterDto.ActiveQuests;
        CompletedTimeSpaces = characterDto.CompletedTimeSpaces;
        MinilandObjects = characterDto.MinilandObjects;
        Inventory = characterDto.Inventory;
        EquippedStuffs = characterDto.EquippedStuffs;
        LifetimeStats = characterDto.LifetimeStats;
        RaidRestrictionDto = characterDto.RaidRestrictionDto;
        RainbowBattleLeaverBusterDto = characterDto.RainbowBattleLeaverBusterDto;
        IcebreakerLeaverBusterDto = characterDto.IcebreakerLeaverBusterDto;
        TrophyFragmentsDto = characterDto.TrophyFragmentsDto;
        BattlePassQuestDto = characterDto.BattlepassQuestDto;
        BattlePassItemDto = characterDto.BattlepassItemDto;
        BattlePassOptionDto = characterDto.BattlePassOptionDto;
        MaxStarTrainerDto = characterDto.MaxStarTrainerDto;
        TrainerQuestDto = characterDto.TrainerQuestDto;
        ItemRestrictionDto = characterDto.ItemRestrictionDto;
        DailyRewardDto = characterDto.DailyRewardDto;
        PityDto = characterDto.PityDto;
        CharacterPrestigeDto =  characterDto.CharacterPrestigeDto;
        LandOfLifeRestrictionDto = characterDto.LandOfLifeRestrictionDto;
        WorldBossRecordsDto = characterDto.WorldBossRecordsDto;
        IsAvailableToChangeName = characterDto.IsAvailableToChangeName;
        FishDto = characterDto.FishDto;
        FishRewardsEarnedDto = characterDto.FishRewardsEarnedDto;
        CharacterCookingDto = characterDto.CharacterCookingDto;
        FoodValue = characterDto.FoodValue;
        _familyManager = familyManager;
        _randomGenerator = randomGenerator;
        _mapManager = mapManager;
        _characterAlgorithm = characterAlgorithm;
        _foodSnackComponentFactory = foodSnackComponentFactory;
        _eventPipeline = eventPipeline;
        _algorithm = algorithm;
        _portalFactory = portalFactory;
        _alzanorComponent = new AlzanorComponent();
        _buffFactory = buffFactory;
        _characterSizeModifiersConfiguration = characterSizeModifiersConfiguration;
        
        _raidComponent = new RaidComponent();
        TimeSpaceComponent = new TimeSpaceComponent();
        _familyComponent = new FamilyComponent(_familyManager);
        _inventory = new InventoryComponent(this, itemsManager);
        _exchange = new ExchangeComponent();
        _characterRevivalComponent = new CharacterRevivalComponent();
        _groupComponent = new GroupComponent();
        _relationComponent = new RelationComponent();
        _eventTriggerContainer = new EventTriggerContainer(_eventPipeline);
        BCardComponent = new BCardComponent(_randomGenerator);
        _questContainer = new BasicQuestContainer();
        BuffComponent = new BuffComponent();
        _eqOptions = new EquipmentOptionContainer();
        _comboSkillComponent = new ComboSkillComponent();
        _bubbleComponent = new BubbleComponent();
        _partnerInventory = new PartnerInventoryComponent(this);
        _castingComponent = new CastingComponent();
        _skillCooldownComponent = new SkillCooldownComponent();
        _chargeComponent = new ChargeComponent();
        _angelElementBuffComponent = new AngelElementBuffComponent();
        _endBuffDamageComponent = new EndBuffDamageComponent();
        _scoutComponent = new ScoutComponent();
        QuicklistComponent = new QuicklistComponent();
        _foodSnackComponent = _foodSnackComponentFactory.CreateFoodSnackComponent();
        HomeComponent = new HomeComponent(characterDto.ReturnPoint);
        MateComponent = new MateComponent();
        WildKeeperComponent = new WildKeeperComponent();
        SkillComponent = new SkillComponent();
        CheatComponent = new CheatComponent();
        SpecialistComponent = new SpecialistStatsComponent(this);
        StatisticsComponent = new PlayerStatisticsComponent(this);
        ChargeComponent = new ChargeComponent();
        ShopComponent = new ShopComponent();
        MailNoteComponent = new MailNoteComponent();
        RainbowBattleComponent = new RainbowBattleComponent();
        BCardDataComponent = new BCardDataComponent();
        HardcoreComponent = new HardcoreComponent();
        BCardStackComponent = new BCardStackComponent();
        AlzanorComponent = new AlzanorComponent();
        RefreshCharacterStats();

        Killer = null;
        SpCooldownEnd = null;
        LastWalk = new LastWalk
        {
            MapId = null
        };

        KillStreak = 0;
        IsInKillStreak = false;

        LastDefence = DateTime.MinValue;
        LastItemUpgrade = DateTime.MinValue;
        LastHealth = DateTime.MinValue;
        LastSkillUse = DateTime.MinValue;
        LastEffect = DateTime.MinValue;
        LastEnergy = DateTime.MinValue;
        LastEnergyRefill = DateTime.MinValue;
        LastTokenEnergy  = DateTime.MinValue;
        LastTokenEnergyRefill  = DateTime.MinValue;
        LastTimeDisplayed = DateTime.MinValue;
        Bubble = DateTime.MinValue;
        ItemsToRemove = DateTime.UtcNow;
        LastPulseTick = DateTime.UtcNow;
        LastAdministrationBazaarRefresh = DateTime.UtcNow;
        LastBuySearchBazaarRefresh = DateTime.UtcNow;
        LastListItemBazaar = DateTime.UtcNow;
        BattlePassProcess = DateTime.UtcNow;
        LastSunWolfTeleport = DateTime.UtcNow;
        LastFoodProcess = DateTime.UtcNow;
        Session = null;
        Skills = new List<IBattleEntitySkill>();
    }
    
    public bool HasUpgradedMartialArtist { get; set; }
    public bool HaveBpUiOpen { get; set; }
    public bool HasCaughtFish { get; set; }
    public bool HasFishingLineBroke { get; set; }
    public bool HasBadLuck{ get; set; }
    public IDisposable FishingFirstFish { get; set; }
    public IDisposable FishingSecondFish { get; set; }
    
    public int FoodValue { get; set; }
    
    public DateTime LastFoodProcess { get; set; }
    
    public List<CharacterCookingDto> CharacterCookingDto { get; set; }
    
    public bool IsCooking { get; set; }
    
    public bool CanCollectMeal { get; set; }
    
    public IDisposable FirstChefAction { get; set; }
    
    public IDisposable SecondChefAction { get; set; }
    
    public IDisposable ThirdChefAction { get; set; }
    
    public Recipe LastRecipeFromChefSp { get; set; }
    
    public int LastSkillId { get; set; }
    public DateTime BattlePassProcess { get; set; }
    public List<CharacterFishRewardsEarnedDto> FishRewardsEarnedDto { get; set; }
    public Tuple<short, short, short> FishSavedLocation { get; set; }
    public List<CharacterFishDto> FishDto { get; set; }
    public short FishInteraction { get; set; }
    public bool CanCollectFish { get; set; }
    public bool IsRareFish { get; set; }
    public bool IsAvailableToChangeName { get; set; }
    public bool WasMorphedPreviously { get; set; }
    public int UntransformPressingKeyCount { get; set; }

    public DateTime LastSlotChange { get; set; }

    public DateTime LastDivinePotion { get; set; }

    public DateTime? GrowthShield { get; set; }

    public int AdditionalHp { get; set; }
    public int AdditionalMp { get; set; }

    public int Hp { get; set; }
    public int MaxHp { get; set; }

    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public bool TriggerAmbush { get; set; }

    public DateTime LastMapChange { get; set; }

    public DateTime? LastSkillCombo { get; set; }

    public AuthorityType Authority { get; set; }

    public int CurrentMinigame { get; set; }

    public byte Level { get; set; }

    public byte Direction { get; set; } = 2;

    public byte HitsReceived { get; set; } = 0;

    public sbyte NoDamageChance { get; set; } = 0;

    public int DamagesMinimum
    {
        get => GetDamagesMinimum(true);
        set { }
    }

    public int DamagesMaximum
    {
        get => GetDamagesMaximum(true);
        set { }
    }

    public int SecondDamageMinimum => GetDamagesMinimum(false);

    public int SecondDamageMaximum => GetDamagesMaximum(false);

    public int HitRate => GetHitRate(true);

    public int HitCriticalChance => GetCriticalChance(true);

    public int HitCriticalDamage => GetCriticalDamage(true);

    public int SecondHitRate => GetHitRate(false);

    public int SecondHitCriticalChance => GetCriticalChance(false);

    public int SecondHitCriticalDamage => GetCriticalDamage(false);

    public int MeleeDefence => this.GetDefence(_meleeDefense, StatisticType.DEFENSE_MELEE);

    public int RangedDefence => this.GetDefence(_rangedDefense, StatisticType.DEFENSE_RANGED);

    public int MagicDefence => this.GetDefence(_magicDefense, StatisticType.DEFENSE_MAGIC);

    public int MeleeDodge => this.GetDodge(_meleeDodge, StatisticType.DODGE_MELEE);

    public int RangedDodge => this.GetDodge(_rangedDodge, StatisticType.DODGE_RANGED);

    public byte Element
    {
        get => Fairy == null ? (byte)0 : Fairy.GameItem.Element;
        set { }
    }

    public int ElementRate
    {
        get => this.GetElement(false);
        set { }
    }

    public int SpecialistElementRate => this.GetElement(true);

    public int FireResistance
    {
        get => this.GetResistance(StatisticType.FIRE);
        set { }
    }

    public int WaterResistance
    {
        get => this.GetResistance(StatisticType.WATER);
        set { }
    }

    public int LightResistance
    {
        get => this.GetResistance(StatisticType.LIGHT);
        set { }
    }

    public int DarkResistance
    {
        get => this.GetResistance(StatisticType.DARK);
        set { }
    }

    public DateTime GameStartDate { get; set; }

    public bool HasShopOpened { get; set; }

    public bool Invisible => this.IsInvisible();

    public bool IsCustomSpeed { get; set; }

    public bool IsShopping { get; set; }

    public bool IsSitting { get; set; }

    public bool IsOnVehicle { get; set; }

    public bool IsMorphed { get; set; }

    public (VisualType, long) LastEntity { get; set; }

    public DateTime? RandomMapTeleport { get; set; }

    public DateTime LastMove { get; set; }

    public DateTime LastPutItem { get; set; }

    public DateTime LastSentNote { get; set; }

    public DateTime? CheckWeedingBuff { get; set; }
    public DateTime LastPvPAttack { get; set; }

    public DateTime LastRainbowArrowEffect { get; set; }
    public DateTime LastRainbowEffects { get; set; }
    
    public DateTime LastIcebreakerEffects { get; set; }
    
    public DateTime LastAct6PvpEffects { get; set; }
    public LastWalk LastWalk { get; set; }

    public int LastNRunId { get; set; }

    public int LastPulse { get; set; }

    public DateTime LastPulseTick { get; set; }

    public DateTime LastDefence { get; set; }

    public DateTime LastItemUpgrade { get; set; }

    public DateTime LastDeath { get; set; }

    public DateTime LastEffect { get; set; }

    public DateTime LastEffectMinigame { get; set; }

    public DateTime LastHealth { get; set; }

    public DateTime LastPortal { get; set; }

    public DateTime LastPotion { get; set; }

    public DateTime LastSnack { get; set; }

    public DateTime LastFood { get; set; }

    public DateTime LastSkillUse { get; set; }

    public DateTime LastSpeedChange { get; set; }

    public DateTime LastTransform { get; set; }

    public DateTime LastEnergy { get; set; }

    public DateTime LastEnergyRefill { get; set; }
    
    public DateTime LastTokenEnergy { get; set; }
    
    public DateTime LastTokenEnergyRefill { get; set; }

    public DateTime? SpCooldownEnd { get; set; }

    public DateTime Bubble { get; set; }

    public DateTime SpyOutStart { get; set; }

    public DateTime ItemsToRemove { get; set; }

    public DateTime BonusesToRemove { get; set; }
    public DateTime LastTimeDisplayed { get; set; }
    public DateTime LastSunWolfTeleport { get; set; }
    public IMapInstance MapInstance => _mapManager.GetMapInstance(MapInstanceId);

    public Guid MapInstanceId { get; set; }

    public IMapInstance Miniland { get; set; }

    public int Morph { get; set; }
    
    public int? PreviousMorph { get; set; }
    public int MorphUpgrade { get; set; }

    public int MorphUpgrade2 { get; set; }

    public int? LastMinilandProducedItem { get; set; }

    public bool IsGettingLosingReputation { get; set; }

    public byte DeathsOnAct4 { get; set; }
    public long ArenaKills { get; set; }
    public long ArenaDeaths { get; set; }

    public TimeSpan? MuteRemainingTime { get; set; }

    public DateTime LastMuteTick { get; set; }
    public DateTime LastSitting { get; set; }

    public DateTime? LastChatMuteMessage { get; set; }
    public DateTime LastInventorySort { get; set; }

    public DateTime? ArenaImmunity { get; set; }

    public int DamageInRaid { get; set; }
    public long InstantCombatDamage { get; set; }

    public byte KillStreak { get; set; }

    public bool IsInKillStreak { get; set; }

    public bool HasNosBazaarOpen { get; set; }
    public bool PreventEnergyRemove { get; set; }
    public bool HasAutoLootEnabled { get; set; }

    public bool CancelUpgrade { get; set; }

    public List<Guid> SignPostMapInstanceIds { get; } = new();

    public Position Position
    {
        get => new(PositionX, PositionY);
        set
        {
            Position pos = value;
            PositionX = pos.X;
            PositionY = pos.Y;
        }
    }

    public short PositionX { get; set; }

    public short PositionY { get; set; }

    public FactionType Faction { get; private set; }

    public IClientSession Session { get; private set; }

    public int Size
    {
        get => _size ?? _characterSizeModifiersConfiguration.SizeTypes
            .First(static x => x.EntityType == EntityType.Player)
            .DefaultSize;
        set => _size = value;
    }
    public List<IBattleEntitySkill> Skills { get; set; }
    public IChargeComponent ChargeComponent { get; }
    public ThreadSafeHashSet<Guid> AggroedEntities { get; } = new();

    public ConcurrentDictionary<int, CharacterSkill> CharacterSkills { get; } = new();

    public ConcurrentDictionary<int, CharacterSkill> SkillsSp { get; set; }

    public byte Speed
    {
        get => _speed;

        set
        {
            LastSpeedChange = DateTime.UtcNow;
            _speed = value > 59 ? (byte)59 : value;
        }
    }

    public IBattleEntity Killer { get; set; }

    public void AddStaticBonus(CharacterStaticBonusDto bonus) => Bonus.Add(bonus);

    public void AddStaticBonuses(IEnumerable<CharacterStaticBonusDto> bonuses) => Bonus.AddRange(bonuses);
    
    public FactionType? CaligorFaction { get; set; }

    public IReadOnlyList<CharacterStaticBonusDto> GetStaticBonuses() => Bonus;

    public CharacterStaticBonusDto GetStaticBonus(Predicate<CharacterStaticBonusDto> predicate)
    {
        return Bonus.FirstOrDefault(x => predicate(x));
    }

    public ConcurrentDictionary<long, int> HitsByMonsters { get; } = new();

    public bool UseSp { get; set; }

    public byte VehicleSpeed { get; set; }

    public byte VehicleMapSpeed { get; set; }

    public int WareHouseSize { get; set; }

    public DateTime LastBuySearchBazaarRefresh { get; set; }

    public DateTime LastBuyBazaarRefresh { get; set; }

    public DateTime LastListItemBazaar { get; set; }

    public DateTime LastAdministrationBazaarRefresh { get; set; }

    public DateTime LastMonsterCaught { get; set; }

    public bool IsSeal { get; set; }

    public bool IsRemovingSpecialistPoints { get; set; }

    public VisualType Type => VisualType.Player;
    public int Id { get; }
    public long AccountId { get; set; }
    public int Act4Dead { get; set; }
    public int Act4Kill { get; set; }
    public int Act4Points { get; set; }
    public int ArenaWinner { get; set; }
    public string Biography { get; set; }
    public bool BuffBlocked { get; set; }
    public bool ShowRaidDeathInfo { get; set; }
    public ClassType Class { get; set; }
    public SubClassType SubClass { get; set; }
    public short Compliment { get; set; }
    public float Dignity { get; set; }
    public bool EmoticonsBlocked { get; set; }
    public bool ExchangeBlocked { get; set; }
    public bool FamilyRequestBlocked { get; set; }
    public bool FriendRequestBlocked { get; set; }
    public GenderType Gender { get; set; }
    public long Gold
    {
        get => _gold;
        set
        {
            _gold = value;
            if (_gold < 0)
            {
                _gold = 0;
            }
        }
    }
    public bool GroupRequestBlocked { get; set; }
    public HairColorType HairColor { get; set; }
    public HairStyleType HairStyle { get; set; }
    public bool HeroChatBlocked { get; set; }
    public byte HeroLevel { get; set; }
    public long HeroXp { get; set; }
    public bool HpBlocked { get; set; }
    public bool IsPetAutoRelive { get; set; }
    public bool IsPartnerAutoRelive { get; set; }
    public byte JobLevel { get; set; }
    public long JobLevelXp { get; set; }
    public long LevelXp { get; set; }
    public int MapId { get; set; }
    public short MapX { get; set; }
    public short MapY { get; set; }
    public int MasterPoints { get; set; }
    public int MasterTicket { get; set; }
    public byte MaxPetCount { get; set; }
    public byte MaxPartnerCount { get; set; }
    public bool MinilandInviteBlocked { get; set; }
    public string MinilandMessage { get; set; }
    public short MinilandPoint { get; set; }
    public MinilandState MinilandState { get; set; }
    public bool MouseAimLock { get; set; }
    public string Name { get; set; }
    public bool QuickGetUp { get; set; }
    public bool HideHat { get; set; }
    public bool HideCD { get; set; }
    public bool HideHPMP { get; set; }
    public bool UiBlocked { get; set; }
    public long Reput { get; set; }
    public byte Slot { get; set; }
    public int SpPointsBonus { get; set; }
    public int SpPointsBasic { get; set; }
    public int EnergyBar { get; set; }
    public int SecondEnergyBar { get; set; }
    public int EnergyRemoved { get; set; }
    
    public int TokenEnergyBar { get; set; }
    
    public int TokenGauge { get; set; }
    
    public int TokenEnergyBarRemoved { get; set; }
    
    public int TokenGaugeRemoved { get; set; }
    public int TalentLose { get; set; }
    public int TalentSurrender { get; set; }
    public int TalentWin { get; set; }
    public bool WhisperBlocked { get; set; }
    public byte TierLevel { get; set; }
    public long TierExperience { get; set; }
    public List<CharacterPartnerInventoryItemDto> PartnerInventory { get; set; }
    public List<MateDTO> NosMates { get; set; }
    public List<long> CompletedTimeSpaces { get; set; }
    public List<PartnerWarehouseItemDto> PartnerWarehouse { get; set; }
    public List<CharacterStaticBonusDto> Bonus { get; set; }
    public List<CharacterStaticBuffDto> StaticBuffs { get; set; }
    public List<CharacterQuicklistEntryDto> Quicklist { get; set; }
    public List<CharacterSkillDTO> LearnedSkills { get; set; }
    public List<CharacterTitleDto> Titles { get; set; }
    public List<CompletedScriptsDto> CompletedScripts { get; set; }
    public List<CharacterQuestDto> CompletedQuests { get; set; }
    public List<CharacterQuestDto> CompletedPeriodicQuests { get; set; }
    public List<CharacterQuestDto> ActiveQuests { get; set; }
    public List<CharacterMinilandObjectDto> MinilandObjects { get; set; }
    public List<CharacterInventoryItemDto> Inventory { get; set; }
    public List<CharacterInventoryItemDto> EquippedStuffs { get; set; }
    public CharacterLifetimeStatsDto LifetimeStats { get; set; }
    public CharacterRaidRestrictionDto RaidRestrictionDto { get; set; }
    public RainbowBattleLeaverBusterDto RainbowBattleLeaverBusterDto { get; set; }
    public IcebreakerLeaverBusterDto IcebreakerLeaverBusterDto { get; set; }
    public CharacterTrophyFragmentsDto TrophyFragmentsDto { get; set; }
    public List<BattlePassItemDto> BattlePassItemDto { get; set; }
    public List<BattlePassQuestDto> BattlePassQuestDto { get; set; }
    public BattlePassOptionDto BattlePassOptionDto { get; set; }
    public List<MaxStarTrainerDto> MaxStarTrainerDto { get; set; }
    public List<TrainerQuestDto> TrainerQuestDto { get; set; }
    public CharacterItemRestrictionDto ItemRestrictionDto { get; set; }

    public IAlzanorComponent AlzanorComponent { get; }
    public List<DailyRewardDto> DailyRewardDto { get; set; }
    public List<CharacterPityDto> PityDto { get; set; }
    public CharacterPrestigeDto CharacterPrestigeDto { get; set; }
    public LandOfLifeRestrictionDto LandOfLifeRestrictionDto { get; set; }
    public List<WorldBossRecordDto> WorldBossRecordsDto { get; set; }
    public InventoryItem LastSeal { get; set; }
    public DateTime LastRaidCreate { get; set; }
    public bool HasBuff(BuffVnums buffVnum) => BuffComponent.HasBuff((short)buffVnum);
    public bool HasBuff(short buffVnum) => BuffComponent.HasBuff(buffVnum);
    public void SetFaction(FactionType faction)
    {
        Faction = faction;
    }

    public void AddEvent(string trigger, IAsyncEvent notification, bool removeOnTrigger = false)
    {
        _eventTriggerContainer.AddEvent(trigger, notification, removeOnTrigger);
    }

    public async Task TriggerEvents(string key) => await _eventTriggerContainer.TriggerEvents(key);

    public async Task EmitEventAsync<T>(T eventArgs) where T : IBattleEntityEvent
    {
        if (eventArgs.Entity != this)
        {
            throw new ArgumentException("An event should be emitted only from the event sender");
        }

        await _eventPipeline.ProcessEventAsync(eventArgs);
    }

    public void EmitEvent<T>(T eventArgs) where T : IBattleEntityEvent
    {
        EmitEventAsync(eventArgs).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public bool HasCompletedQuest(int questId) => _questContainer.HasCompletedQuest(questId);
    public void RemoveCompletedQuest(int questId) => _questContainer.RemoveCompletedQuest(questId);

    public void RemoveCompletedScript(int scriptId, int scriptIndex) => _questContainer.RemoveCompletedScript(scriptId, scriptIndex);

    public void RemoveAllCompletedScripts() => _questContainer.RemoveAllCompletedScripts();
    public void ClearCompletedPeriodicQuests() => _questContainer.ClearCompletedPeriodicQuests();

    public IHomeComponent HomeComponent { get; }
    public ICheatComponent CheatComponent { get; }

    protected bool Equals(IPlayerEntity other) => Id == other.Id && Type == other.Type;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((IPlayerEntity)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Id.GetHashCode() * 397 ^ Type.GetHashCode();
        }
    }

    #region Methods

    private int GetDamagesMinimum(bool isMainWeapon)
    {
        return Class switch
        {
            ClassType.Swordman => this.GetDamage(isMainWeapon ? _meleeDamageMin : _rangedDamageMin, true, isMainWeapon ? StatisticType.ATTACK_MELEE : StatisticType.ATTACK_RANGED, isMainWeapon),
            ClassType.MartialArtist => this.GetDamage(isMainWeapon ? _meleeDamageMax : _rangedDamageMax, true, isMainWeapon ? StatisticType.ATTACK_MELEE : StatisticType.ATTACK_RANGED, isMainWeapon),
            ClassType.Adventurer => this.GetDamage(isMainWeapon ? _meleeDamageMin : _rangedDamageMin, true, isMainWeapon ? StatisticType.ATTACK_MELEE : StatisticType.ATTACK_RANGED, isMainWeapon),
            ClassType.Archer => this.GetDamage(isMainWeapon ? _rangedDamageMin : _meleeDamageMin, true, isMainWeapon ? StatisticType.ATTACK_RANGED : StatisticType.ATTACK_MELEE, isMainWeapon),
            ClassType.Magician => this.GetDamage(isMainWeapon ? _magicDamageMin : _rangedDamageMin, true, isMainWeapon ? StatisticType.ATTACK_MAGIC : StatisticType.ATTACK_RANGED, isMainWeapon)
        };
    }

    private int GetHitRate(bool isMainWeapon)
    {
        int hitRate = 0;
        if (Class == ClassType.Magician && isMainWeapon)
        {
            return this.GetHitRate(0, true, StatisticType.HITRATE_MAGIC);
        }

        if (isMainWeapon)
        {
            switch (Class)
            {
                case ClassType.Swordman:
                case ClassType.MartialArtist:
                case ClassType.Adventurer:
                    hitRate = this.GetHitRate(_meleeHitRate, true, StatisticType.HITRATE_MELEE);
                    break;
                case ClassType.Archer:
                    hitRate = this.GetHitRate(_rangedHitRate, true, StatisticType.HITRATE_RANGED);
                    break;
            }
        }
        else
        {
            switch (Class)
            {
                case ClassType.MartialArtist:
                case ClassType.Archer:
                case ClassType.Adventurer:
                    hitRate = this.GetHitRate(_meleeHitRate, false, StatisticType.HITRATE_MELEE);
                    break;
                case ClassType.Magician:
                case ClassType.Swordman:
                    hitRate = this.GetHitRate(_rangedHitRate, false, StatisticType.HITRATE_RANGED);
                    break;
            }
        }

        return hitRate;
    }

    private int GetCriticalChance(bool isMainWeapon)
    {
        if (Class == ClassType.Magician && isMainWeapon)
        {
            return 0;
        }

        return this.GetCriticalChance(_criticalChance, isMainWeapon);
    }

    private int GetCriticalDamage(bool isMainWeapon)
    {
        if (Class == ClassType.Magician && isMainWeapon)
        {
            return 0;
        }

        return this.GetCriticalDamage(_criticalDamage, isMainWeapon);
    }

    private int GetDamagesMaximum(bool isMainWeapon)
    {
        return Class switch
        {
            ClassType.Swordman =>
                this.GetDamage(isMainWeapon ? _meleeDamageMax : _rangedDamageMax, false, isMainWeapon ? StatisticType.ATTACK_MELEE : StatisticType.ATTACK_RANGED, isMainWeapon),
            ClassType.MartialArtist =>
                this.GetDamage(isMainWeapon ? _meleeDamageMax : _rangedDamageMax, false, isMainWeapon ? StatisticType.ATTACK_MELEE : StatisticType.ATTACK_RANGED, isMainWeapon),
            ClassType.Adventurer =>
                this.GetDamage(isMainWeapon ? _meleeDamageMax : _rangedDamageMax, false, isMainWeapon ? StatisticType.ATTACK_MELEE : StatisticType.ATTACK_RANGED, isMainWeapon),
            ClassType.Archer =>
                this.GetDamage(isMainWeapon ? _rangedDamageMax : _meleeDamageMax, false, isMainWeapon ? StatisticType.ATTACK_RANGED : StatisticType.ATTACK_MELEE, isMainWeapon),
            ClassType.Magician =>
                this.GetDamage(isMainWeapon ? _magicDamageMax : _rangedDamageMax, false, isMainWeapon ? StatisticType.ATTACK_MAGIC : StatisticType.ATTACK_RANGED, isMainWeapon)
        };
    }

    public bool IsCraftingItem { get; set; }
    public bool IsBankOpen { get; set; }
    public bool IsSunWolfDead { get; set; }
    public DateTime LastUnfreezedPlayer { get; set; }
    public DateTime LastUpgradePet { get; set; }
    public DateTime LastSpPacketSent { get; set; }
    public DateTime LastSpRemovingProcess { get; set; }
    public DateTime LastSpPointProcess { get; set; }
    public DateTime LastAttack { get; set; }
    public bool InitialScpPacketSent { get; set; }
    public bool BlockAllAttack { get; set; }
    public bool IsDraconicMorphed { get; set; }
    public byte UsageSkillWithoutCd { get; set; }
    public bool IsFlameDruidTransformed { get; set; }
    public Guid? UnderWaterShowdownId { get; set; }
    
    public DateTime LastFlameDruid { get; set; }
    public DateTime LastDraconicMorph { get; set; }
    public IQuicklistComponent QuicklistComponent { get; }
    public ISpecialistStatsComponent SpecialistComponent { get; }
    public IPlayerStatisticsComponent StatisticsComponent { get; }
    public IRainbowBattleComponent RainbowBattleComponent { get; }
    public IBCardDataComponent BCardDataComponent { get; }
    public IHardcoreComponent HardcoreComponent { get; }
    public PrivateMapInstanceInfo PrivateMapInstanceInfo { get; set; }
    public IBCardStackComponent BCardStackComponent { get; }
    public int GetCp()
    {
        int cpMax = (Class > 0 ? 40 : 0) + JobLevel * 2;
        int cpUsed = 0;

        foreach (CharacterSkill skill in CharacterSkills.Values)
        {
            if (skill == null)
            {
                continue;
            }

            if (skill.Skill.IsPassiveSkill())
            {
                continue;
            }

            cpUsed += skill.Skill.CPCost;
        }

        return cpMax - cpUsed;
    }

    public bool IsQuestActive(int questId) => ActiveQuests.Any(q => q.QuestId == questId);

    public int GetDignityIco()
    {
        int icoDignity = 1;

        if (Dignity <= -100)
        {
            icoDignity = 2;
        }

        if (Dignity <= -200)
        {
            icoDignity = 3;
        }

        if (Dignity <= -400)
        {
            icoDignity = 4;
        }

        if (Dignity <= -600)
        {
            icoDignity = 5;
        }

        if (Dignity <= -800)
        {
            icoDignity = 6;
        }

        return icoDignity;
    }

    public List<IPortalEntity> GetExtraPortal()
    {
        if (MapInstance == null || Miniland == null)
        {
            return new List<IPortalEntity>();
        }

        return MapInstance.GenerateMinilandEntryPortals(Miniland, _portalFactory);
    }

    [Obsolete("Move to constructor")]
    public void SetSession(IClientSession clientSession)
    {
        Session = clientSession;
    }

    public int HealthHpLoad()
    {
        if (IsSitting)
        {
            return _characterAlgorithm.GetRegenHp(this, Class, true);
        }

        return LastDefence.AddSeconds(4) <= DateTime.UtcNow ? _characterAlgorithm.GetRegenHp(this, Class, false) : 0;
    }

    public int HealthMpLoad()
    {
        if (IsSitting)
        {
            return _characterAlgorithm.GetRegenMp(this, Class, true);
        }

        return LastDefence.AddSeconds(4) <= DateTime.UtcNow ? _characterAlgorithm.GetRegenMp(this, Class, false) : 0;
    }
    
    public bool HasUnlockedDesiredBearing(IReadOnlyList<Recipe> recipes, byte bearing)
    {
        var bearings = new List<bool>();

        foreach (Recipe i in recipes.Where(s => s.BearingChef == bearing))
        {
            CharacterCookingDto logBearing = CharacterCookingDto.FirstOrDefault(s => s.RecipeVnum == i.ProducedItemVnum);
            
            if (logBearing == null)
            {
                bearings.Add(false);
                continue;
            }
            
            if (logBearing.Amount < i.LimitCrafting)
            {
                bearings.Add(false);
                continue;
            }
            
            bearings.Add(true);
        }

        return bearings.All(s => s);
    }

    #endregion
}