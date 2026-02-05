// NosEmu
// 


using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Skills;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Mates;

public partial class MateEntity : IMateEntity
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICastingComponent _castingComponent;
    private readonly IEndBuffDamageComponent _endBuffDamageComponent;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IEventTriggerContainer _eventTriggerContainer;
    private readonly IReadOnlyList<NpcMonsterSkillDTO> _npcMonsterSkills;
    private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;
    
    private int? _size;

    public MateEntity(IPlayerEntity owner, MonsterData npcMonster, byte level, byte heroLevel, List<int> trainerSkills, MateType mateType,
        IMateTransportFactory transportFactory, IAsyncEventPipeline eventPipeline, IBattleEntityAlgorithmService algorithm,
        IRandomGenerator randomGenerator, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
    {
        Owner = owner;
        NpcMonsterVNum = npcMonster.Id;
        Level = level;
        HeroLevel = heroLevel;
        MateName = npcMonster.Name;
        MateType = mateType;
        Loyalty = 1000;
        CharacterId = owner.Id;
        Id = transportFactory.GenerateTransportId();
        TrainerSkills = trainerSkills;
        _characterSizeModifiersConfiguration  = characterSizeModifiersConfiguration;

        #region NpcMonsterData

        BaseStarLevelTaming = npcMonster.BaseStarLevelTaming;
        ArmorLevel = npcMonster.ArmorLevel;
        AttackType = npcMonster.AttackType;
        AttackUpgrade = npcMonster.AttackUpgrade;
        AttackEffect = npcMonster.AttackEffect;
        BasicCastTime = npcMonster.BasicCastTime;
        BasicCooldown = npcMonster.BasicCooldown;
        BasicRange = npcMonster.BasicRange;
        BCards = npcMonster.BCards;
        BasicHitChance = npcMonster.BasicHitChance;
        CellSize = npcMonster.CellSize;
        CleanDamageMin = npcMonster.CleanDamageMin;
        CleanDamageMax = npcMonster.CleanDamageMax;
        CleanHitRate = npcMonster.CleanHitRate;
        CleanMeleeDefence = npcMonster.CleanMeleeDefence;
        CleanRangeDefence = npcMonster.CleanRangeDefence;
        CleanMagicDefence = npcMonster.CleanMagicDefence;
        CleanDodge = npcMonster.CleanDodge;
        CleanHp = npcMonster.CleanHp;
        CleanMp = npcMonster.CleanMp;
        BaseCloseDefence = npcMonster.BaseCloseDefence;
        BaseConcentrate = npcMonster.BaseConcentrate;
        BaseCriticalChance = npcMonster.BaseCriticalChance;
        BaseCriticalRate = npcMonster.BaseCriticalRate;
        BaseDamageMaximum = npcMonster.BaseDamageMaximum;
        BaseDamageMinimum = npcMonster.BaseDamageMinimum;
        BaseDarkResistance = npcMonster.BaseDarkResistance;
        BaseMaxHp = npcMonster.BaseMaxHp;
        BaseMaxMp = npcMonster.BaseMaxMp;
        DefenceDodge = npcMonster.DefenceDodge;
        DefenceUpgrade = npcMonster.DefenceUpgrade;
        DistanceDefence = npcMonster.DistanceDefence;
        DistanceDefenceDodge = npcMonster.DistanceDefenceDodge;
        BaseElement = npcMonster.BaseElement;
        BaseElementRate = npcMonster.BaseElementRate;
        BaseFireResistance = npcMonster.BaseFireResistance;
        BaseLevel = npcMonster.BaseLevel;
        BaseHeroLevel = npcMonster.BaseHeroLevel;
        BaseLightResistance = npcMonster.BaseLightResistance;
        MagicDefence = npcMonster.MagicDefence;
        MagicMpFactor = npcMonster.MagicMpFactor;
        MeleeHpFactor = npcMonster.MeleeHpFactor;
        MinimumAttackRange = npcMonster.MinimumAttackRange;
        MonsterVNum = npcMonster.MonsterVNum;
        MateName = npcMonster.Name;
        MonsterRaceType = npcMonster.MonsterRaceType;
        MonsterRaceSubType = npcMonster.MonsterRaceSubType;
        RangeDodgeFactor = npcMonster.RangeDodgeFactor;
        _speed = npcMonster.BaseSpeed;
        BaseWaterResistance = npcMonster.BaseWaterResistance;
        WeaponLevel = npcMonster.WeaponLevel;
        WinfoValue = npcMonster.WinfoValue;
        Name = npcMonster.Name;
        MonsterSkills = npcMonster.MonsterSkills;
        BaseXp = npcMonster.BaseXp;
        BaseJobXp = npcMonster.BaseJobXp;
        _npcMonsterSkills = npcMonster.Skills;

        #endregion

        _eventPipeline = eventPipeline;
        _algorithm = algorithm;
        _revivalComponent = new MateRevivalComponent();
        BCardComponent = new BCardComponent(randomGenerator);
        BuffComponent = new BuffComponent();
        _eventTriggerContainer = new EventTriggerContainer(_eventPipeline);
        _castingComponent = new CastingComponent();
        _endBuffDamageComponent = new EndBuffDamageComponent();
        StatisticsComponent = new MateStatisticsComponent(this);
        ChargeComponent = new ChargeComponent();
        BCardStackComponent = new BCardStackComponent();
        BCardDataComponent = new BCardDataComponent();
        AlzanorComponent = new AlzanorComponent();
    }

    private IMateRevivalComponent _revivalComponent { get; }

    public bool TeleportBackOnNoticeRangeExceed { get; }

    public DateTime LastSpeedChange { get; set; }

    public int Size
    {
        get => _size ?? _characterSizeModifiersConfiguration.SizeTypes
            .First(static x => x.EntityType == EntityType.Mate)
            .DefaultSize;
        set => _size = value;
    }

    public byte Element
    {
        get => IsUsingSp && Specialist != null ? Specialist.GameItem.Element : BaseElement;
        set { }
    }

    public int ElementRate
    {
        get => IsUsingSp && Specialist != null ? Specialist.GameItem.ElementRate : BaseElementRate;
        set { }
    }

    public int FireResistance
    {
        get => GetMoreStats(StatisticType.FIRE, BaseFireResistance);
        set { }
    }

    public int WaterResistance
    {
        get => GetMoreStats(StatisticType.WATER, BaseWaterResistance);
        set { }
    }

    public int LightResistance
    {
        get => GetMoreStats(StatisticType.LIGHT, BaseLightResistance);
        set { }
    }

    public int DarkResistance
    {
        get => GetMoreStats(StatisticType.DARK, BaseDarkResistance);
        set { }
    }

    public int DamagesMinimum
    {
        get => GetMateDamage(_damageMin, true);
        set { }
    }

    public int DamagesMaximum
    {
        get => GetMateDamage(_damageMax, false);
        set { }
    }

    public short CloseDefence
    {
        get => GetMoreStats(StatisticType.DEFENSE_MELEE, _meleeDefense);
        set { }
    }

    public short DistanceDefence
    {
        get => GetMoreStats(StatisticType.DEFENSE_RANGED, _rangedDefense);
        set { }
    }

    public short MagicDefence
    {
        get => GetMoreStats(StatisticType.DEFENSE_MAGIC, _magicDefense);
        set { }
    }

    public int BaseXp { get; }
    public int BaseJobXp { get; }

    public short DefenceDodge
    {
        get => GetMoreStats(StatisticType.DODGE_MELEE, _meleeDodge);
        set { }
    }

    public short DistanceDodge
    {
        get => GetMoreStats(StatisticType.DODGE_RANGED, _rangedDodge);
        set { }
    }

    public short HitRate
    {
        get => (short)GetMateHitRate(_hitRate);
        set { }
    }

    public int HitCriticalChance
    {
        get => GetMateCritical(_criticalChance, true);
        set { }
    }

    public int HitCriticalDamage
    {
        get => GetMateCritical(_criticalDamage, false);
        set { }
    }

    public int MaxHp
    {
        get => this.GetMaxHp(_maxHp);

        set => _maxHp = value;
    }

    public int MaxMp
    {
        get => this.GetMaxMp(_maxMp);

        set => _maxMp = value;
    }

    public byte Speed
    {
        get => this.GetSpeed(_speed);

        set
        {
            LastSpeedChange = DateTime.UtcNow;
            _speed = value > 59 ? (byte)59 : value;
        }
    }

    public IBCardComponent BCardComponent { get; }
    public IChargeComponent ChargeComponent { get; }
    
    public IBCardStackComponent BCardStackComponent { get; }
    public ThreadSafeHashSet<Guid> AggroedEntities { get; } = new();
    public IBuffComponent BuffComponent { get; }
    public IBCardDataComponent BCardDataComponent { get; }

    public IAlzanorComponent AlzanorComponent { get; }

    public FactionType Faction => Owner?.Faction ?? FactionType.Neutral;

    public bool IsSitting { get; set; }

    public bool IsUsingSp { get; set; }

    public short MinilandY { get; set; }

    public DateTime LastHealth { get; set; }

    public DateTime LastDeath { get; set; }

    public DateTime LastDefence { get; set; }

    public DateTime LastBasicSkill { get; set; }

    public DateTime? SpawnMateByGuardian { get; set; }

    public int? SavedGuardianVnum { get; set; }

    public DateTime LastSkillUse { get; set; }

    public IMapInstance MapInstance => IsTeamMember ? Owner?.MapInstance : Owner?.Miniland;

    public IPlayerEntity Owner { get; set; }

    public byte PetSlot { get; set; }

    public List<IBattleEntitySkill> Skills { get; set; }

    public Position Position
    {
        get => new(PositionX, PositionY);
        set
        {
            PositionX = value.X;
            PositionY = value.Y;
        }
    }

    public short PositionX { get; set; }

    public short PositionY { get; set; }

    public IBattleEntitySkill LastUsedPartnerSkill { get; set; }

    public IBattleEntity Killer { get; set; }

    public bool IsLimited { get; init; }
    public int HitsAgainstDoll { get; set; }
    public int HitsFromDoll { get; set; }
    public VisualType TargetVisualType { get; set; }
    public long TargetId { get; set; }
    public DateTime? SpCooldownEnd { get; set; }
    public DateTime LastEffect { get; set; }
    public DateTime LastPetUpgradeEffect { get; set; }
    public GameItemInstance Weapon => Owner?.PartnerGetEquippedItem(EquipmentType.MainWeapon, PetSlot)?.ItemInstance;
    public GameItemInstance Armor => Owner?.PartnerGetEquippedItem(EquipmentType.Armor, PetSlot)?.ItemInstance;
    public GameItemInstance Gloves => Owner?.PartnerGetEquippedItem(EquipmentType.Gloves, PetSlot)?.ItemInstance;
    public GameItemInstance Boots => Owner?.PartnerGetEquippedItem(EquipmentType.Boots, PetSlot)?.ItemInstance;
    public GameItemInstance Specialist => Owner?.PartnerGetEquippedItem(EquipmentType.Sp, PetSlot)?.ItemInstance;

    public void Initialize()
    {
        RefreshStatistics();

        if (Hp == 0)
        {
            Hp = _maxHp;
            Mp = _maxMp;
        }

        Skills = new List<IBattleEntitySkill>();
        foreach (INpcMonsterSkill skill in MonsterSkills)
        {
            var tmp = new NpcMonsterSkill(skill.Skill, skill.Rate, skill.IsBasicAttack, skill.IsIgnoringHitChance);
            Skills.Add(tmp);
        }

        foreach (NpcMonsterSkillDTO skill in _npcMonsterSkills)
        {
            var monsterSkill = new NpcMonsterSkill(StaticSkillsManager.Instance.GetSkill(skill.SkillVNum), skill.Rate, skill.IsBasicAttack, skill.IsIgnoringHitChance);
            Skills.Add(monsterSkill);
        }
        
        foreach (int skillId in TrainerSkills)
        {
            var monsterSkill = new NpcMonsterSkill(StaticSkillsManager.Instance.GetSkill(skillId), 100, false, false);
            Skills.Add(monsterSkill);
        }


        this.InitializeBCards();
        Killer = null;
        LastBasicSkill = DateTime.MinValue;

        BasicSkill = new SkillInfo
        {
            Vnum = 0,
            AttackType = AttackType,
            SkillType = SkillType.PartnerSkill,
            CastTime = BasicCastTime,
            Cooldown = BasicCooldown,
            Element = Element,
            HitAnimation = 11,
            HitEffect = AttackEffect,
            HitChance = BasicHitChance,
            TargetType = TargetType.Target,
            TargetAffectedEntities = TargetAffectedEntities.Enemies,
            Range = BasicRange,
            Class = 0
        };

        var dictionary = new Dictionary<SkillCastType, HashSet<BCardDTO>>();
        foreach (BCardDTO bCard in BCardComponent.GetTriggerBCards(BCardTriggerType.ATTACK))
        {
            var key = (SkillCastType)bCard.CastType;
            if (!dictionary.TryGetValue(key, out HashSet<BCardDTO> hashSet))
            {
                hashSet = new HashSet<BCardDTO>();
                dictionary[key] = hashSet;
            }

            hashSet.Add(bCard);
        }

        BasicSkill.BCardsType = dictionary;
    }

    public SkillInfo BasicSkill { get; private set; }

    public IMateStatisticsComponent StatisticsComponent { get; }
    public short? SpecialSkillVnum { get; set; }

    public DateTime LastLowLoyaltyEffect { get; set; } = DateTime.MinValue;

    public DateTime LastLoyaltyRecover { get; set; } = DateTime.MinValue;

    public VisualType Type => VisualType.Npc;

    public int Id { get; }

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false) => _eventTriggerContainer.AddEvent(key, notification, removedOnTrigger);

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

    public byte Attack { get; set; }

    public bool CanPickUp { get; set; }

    public long CharacterId { get; set; }

    public byte Defence { get; set; }

    public byte Direction { get; set; } = 2;

    public byte HitsReceived { get; set; } = 0;

    public sbyte NoDamageChance { get; set; } = 0;

    public long Experience { get; set; }

    public int Hp { get; set; }

    public bool IsSummonable { get; set; }

    public bool IsTeamMember { get; set; }

    public byte Level { get; set; }
    
    public byte HeroLevel { get; set; }

    public byte Stars { get; set; }

    public int TrainerExperience { get; set; }
    
    public bool IsTemporaryPet { get; set; }
    
    public List<int> TrainerSkills { get; set; }

    public short Loyalty { get; set; }

    public short MapX { get; set; }

    public short MapY { get; set; }

    public short MinilandX { get; set; }

    public MateType MateType { get; set; }

    public int Mp { get; set; }

    public string MateName { get; set; }

    public int NpcMonsterVNum { get; set; }

    public short Skin { get; set; }
    
    public byte BaseStarLevelTaming { get; set; }
    public bool HasDhaPremium { get; set; }
    public bool IsDhaLootEnabled { get; set; }

    public List<long> SavedDrops { get; set; } = new();

    public DateTime RevivalDateTimeForExecution => _revivalComponent.RevivalDateTimeForExecution;

    public bool IsRevivalDelayed => _revivalComponent.IsRevivalDelayed;

    public void UpdateRevival(DateTime revivalDateTimeForExecution, bool isRevivalDelayed)
    {
        _revivalComponent.UpdateRevival(revivalDateTimeForExecution, isRevivalDelayed);
    }

    public void DisableRevival()
    {
        _revivalComponent.DisableRevival();
    }

    public SkillCast SkillCast => _castingComponent.SkillCast;

    public bool IsCastingSkill => _castingComponent.IsCastingSkill;

    public void SetCastingSkill(SkillInfo skill, DateTime time)
    {
        _castingComponent.SetCastingSkill(skill, time);
    }

    public void RemoveCastingSkill()
    {
        _castingComponent.RemoveCastingSkill();
    }

    public IReadOnlyDictionary<short, int> EndBuffDamages => _endBuffDamageComponent.EndBuffDamages;
    public void AddEndBuff(short buffVnum, int damage)
    {
        _endBuffDamageComponent.AddEndBuff(buffVnum, damage);
    }

    public int DecreaseDamageEndBuff(short buffVnum, int damage) => _endBuffDamageComponent.DecreaseDamageEndBuff(buffVnum, damage);

    public void RemoveEndBuffDamage(short buffVnum)
    {
        _endBuffDamageComponent.RemoveEndBuffDamage(buffVnum);
    }
}