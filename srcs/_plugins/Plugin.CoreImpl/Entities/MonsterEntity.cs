// NosEmu
// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Drops;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace Plugin.CoreImpl.Entities
{
    public class MonsterEntity : IMonsterEntity
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IBattleEntityAlgorithmService _battleEntityAlgorithmService;
        private readonly ICastingComponent _castingComponent;
        private readonly IEndBuffDamageComponent _endBuffDamageComponent;
        private readonly IEventTriggerContainer _eventContainer;
        private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;

        private double _enhancedHp = 1;
        private int? _size;
        public MonsterEntity(int id, IEventTriggerContainer eventContainer, IBCardComponent bCardComponent, IAsyncEventPipeline asyncEventPipeline, IMonsterData monsterData, IMapInstance mapInstance,
            MonsterEntityBuilder builder, IReadOnlyList<INpcMonsterSkill> npcMonsterSkills, IBattleEntityAlgorithmService battleEntityAlgorithmService, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
        {
            UniqueId = builder?.GeneratedGuid ?? Guid.NewGuid();
            BCardComponent = bCardComponent;
            _castingComponent = new CastingComponent();
            Damagers = new ThreadSafeHashSet<IBattleEntity>();
            _endBuffDamageComponent = new EndBuffDamageComponent();
            ChargeComponent = new ChargeComponent();
            AlzanorComponent = new AlzanorComponent();
            BCardStackComponent = new BCardStackComponent();
            _eventContainer = eventContainer;
            BuffComponent = new BuffComponent();
            BCardDataComponent = new BCardDataComponent();
            _asyncEventPipeline = asyncEventPipeline;
            _battleEntityAlgorithmService = battleEntityAlgorithmService;
            _characterSizeModifiersConfiguration =  characterSizeModifiersConfiguration;

            Id = id;
            AmountRequired = monsterData.AmountRequired;
            ArmorLevel = monsterData.ArmorLevel;
            AttackType = monsterData.AttackType;
            AttackUpgrade = monsterData.AttackUpgrade;
            BasicCastTime = monsterData.BasicCastTime;
            BasicCooldown = monsterData.BasicCooldown;
            BasicRange = monsterData.BasicRange;
            BCards = monsterData.BCards;
            AttackEffect = monsterData.AttackEffect;
            BasicHitChance = monsterData.BasicHitChance;
            CanBeCollected = monsterData.CanBeCollected;
            CanBeDebuffed = monsterData.CanBeDebuffed;
            CanBeCaught = monsterData.CanBeCaught;
            CanBePushed = monsterData.CanBePushed;
            CanRegenMp = monsterData.CanRegenMp;
            CellSize = monsterData.CellSize;
            CleanDamageMin = monsterData.CleanDamageMin;
            CleanDamageMax = monsterData.CleanDamageMax;
            CleanHitRate = monsterData.CleanHitRate;
            CleanMeleeDefence = monsterData.CleanMeleeDefence;
            CleanRangeDefence = monsterData.CleanRangeDefence;
            CleanMagicDefence = monsterData.CleanMagicDefence;
            CleanDodge = monsterData.CleanDodge;
            CleanHp = monsterData.CleanHp;
            CleanMp = monsterData.CleanMp;
            BaseCloseDefence = monsterData.BaseCloseDefence;
            BaseConcentrate = monsterData.BaseConcentrate;
            BaseCriticalChance = monsterData.BaseCriticalChance;
            BaseCriticalRate = monsterData.BaseCriticalRate;
            DamagedOnlyLastJajamaruSkill = monsterData.DamagedOnlyLastJajamaruSkill;
            BaseDamageMinimum = monsterData.BaseDamageMaximum;
            BaseDamageMaximum = monsterData.BaseDamageMinimum;
            BaseDarkResistance = monsterData.BaseDarkResistance;
            DeathEffect = monsterData.DeathEffect;
            BaseMaxHp = monsterData.BaseMaxHp;
            BaseMaxMp = monsterData.BaseMaxMp;
            DefenceDodge = monsterData.DefenceDodge;
            DefenceUpgrade = monsterData.DefenceUpgrade;
            Drops = monsterData.Drops;
            DisappearAfterHitting = monsterData.DisappearAfterHitting;
            DisappearAfterSeconds = monsterData.DisappearAfterSeconds;
            DisappearAfterSecondsMana = monsterData.DisappearAfterSecondsMana;
            DistanceDefence = monsterData.DistanceDefence;
            DistanceDefenceDodge = monsterData.DistanceDefenceDodge;
            BaseElement = monsterData.BaseElement;
            BaseElementRate = monsterData.BaseElementRate;
            BaseFireResistance = monsterData.BaseFireResistance;
            GiveDamagePercentage = monsterData.GiveDamagePercentage;
            GroupAttack = monsterData.GroupAttack;
            HasMode = monsterData.HasMode;
            RawHostility = monsterData.RawHostility;
            IconId = monsterData.IconId;
            IsPercent = monsterData.IsPercent;
            JobXp = monsterData.JobXp;
            BaseLevel = monsterData.BaseLevel;
            BaseHeroLevel = monsterData.BaseHeroLevel;
            BaseLightResistance = monsterData.BaseLightResistance;
            MagicDefence = monsterData.MagicDefence;
            MagicMpFactor = monsterData.MagicMpFactor;
            MeleeHpFactor = monsterData.MeleeHpFactor;
            Stars = monsterData.Stars;
            MinimumAttackRange = monsterData.MinimumAttackRange;
            MonsterVNum = monsterData.MonsterVNum;
            Name = monsterData.Name;
            NoticeRange = monsterData.NoticeRange;
            OnDefenseOnlyOnce = monsterData.OnDefenseOnlyOnce;
            PermanentEffect = monsterData.PermanentEffect;
            MonsterRaceType = monsterData.MonsterRaceType;
            MonsterRaceSubType = monsterData.MonsterRaceSubType;
            RangeDodgeFactor = monsterData.RangeDodgeFactor;
            BaseRespawnTime = monsterData.BaseRespawnTime;
            SpawnMobOrColor = monsterData.SpawnMobOrColor;
            BaseSpeed = monsterData.BaseSpeed;
            SpriteSize = monsterData.SpriteSize;
            TakeDamages = monsterData.TakeDamages;
            VNumRequired = monsterData.VNumRequired;
            BaseWaterResistance = monsterData.BaseWaterResistance;
            WeaponLevel = monsterData.WeaponLevel;
            WinfoValue = monsterData.WinfoValue;
            Xp = monsterData.Xp;
            MaxTries = monsterData.MaxTries;
            CollectionCooldown = monsterData.CollectionCooldown;
            CollectionDanceTime = monsterData.CollectionDanceTime;
            TeleportRemoveFromInventory = monsterData.TeleportRemoveFromInventory;
            BasicDashSpeed = monsterData.BasicDashSpeed;
            ModeIsHpTriggered = monsterData.ModeIsHpTriggered;
            ModeLimiterType = monsterData.ModeLimiterType;
            ModeRangeTreshold = monsterData.ModeRangeTreshold;
            ModeCModeVnum = monsterData.ModeCModeVnum;
            ModeHpTresholdOrItemVnum = monsterData.ModeHpTresholdOrItemVnum;
            MidgardDamage = monsterData.MidgardDamage;
            HasDash = monsterData.HasDash;
            DropToInventory = monsterData.DropToInventory;
            ModeBCards = monsterData.ModeBCards;
            BaseXp = monsterData.BaseXp;
            BaseJobXp = monsterData.BaseJobXp;
            LastEffect = Death = SpawnDate = DateTime.UtcNow;
            LastSkill = DateTime.MinValue;
            IsHostile = RawHostility != (int)HostilityType.NOT_HOSTILE;
            IsJumping = false;
            LastJump = DateTime.UtcNow;
            HasSpawnRedCircle = false;
            IsTotem = builder.IsTotem;
            IsStillAlive = true;
            Speed = this.GetSpeed(BaseSpeed);
            Hp = BaseMaxHp;
            Mp = BaseMaxMp;
            Level = BaseLevel;
            HeroLevel = BaseHeroLevel;
            CanSeeInvisible = monsterData.CanSeeInvisible;
            
            Faction = RawHostility switch
            {
                (int)HostilityType.ATTACK_ANGELS_ONLY => FactionType.Demon,
                (int)HostilityType.ATTACK_DEVILS_ONLY => FactionType.Angel,
                _ => monsterData.SuggestedFaction ?? FactionType.Neutral
            };

            if (builder != null)
            {
                PositionX = FirstX = builder.PositionX;
                PositionY = FirstY = builder.PositionY;
                Direction = builder.Direction;
                CanWalk = builder.IsWalkingAround & monsterData.CanWalk;
                BaseShouldRespawn = builder.IsRespawningOnDeath;
                IsMateTrainer = builder.IsMateTrainer;
                IsBonus = builder.IsBonus;
                IsBoss = builder.IsBoss;
                IsTarget = builder.IsTarget;
                IsSparringMonster = builder.IsSparringMonster;
                VesselMonster = builder.IsVesselMonster;
                VesselChristmasMonster = builder.IsVesselChristmasMonster;
                IgnoreSkillRange = builder.IgnoreSkillRange;
                SummonerId = builder.SummonerId;
                SummonerType = builder.SummonerType;
                IsHostile = builder.IsHostile;
                SummonType = builder.SummonType;
                GoToBossPosition = builder.GoToBossPosition;
                IsInstantBattle = builder.IsInstantBattle;
                RaidDrop = builder.RaidDrop;

                if (builder.Level.HasValue)
                {
                    Level = builder.Level.Value;
                    UpdateMonstersBaseStatistics();
                    Hp = MaxHp;
                    Mp = MaxMp;
                }
                
                if (builder.HeroLevel.HasValue)
                {
                    HeroLevel = builder.HeroLevel.Value;
                    UpdateMonstersBaseStatistics();
                    Hp = MaxHp;
                    Mp = MaxMp;
                }

                if (builder.HpMultiplier.HasValue)
                {
                    BaseMaxHp = (int)(BaseMaxHp * builder.HpMultiplier.Value);
                    Hp = BaseMaxHp;
                }

                if (builder.MpMultiplier.HasValue)
                {
                    BaseMaxMp = (int)(BaseMaxMp * builder.MpMultiplier.Value);
                    Mp = BaseMaxMp;
                }

                Faction = builder.FactionType ?? (SuggestedFaction ?? FactionType.Neutral);
            }

            Drops = monsterData.Drops;
            MonsterSkills = npcMonsterSkills;

            NotBasicSkills = MonsterSkills.Where(x => !x.IsBasicAttack).ToArray();
            if (HasDash)
            {
                DashSkill = MonsterSkills.Count != 0 ? MonsterSkills[0] : null;
            }

            SkillsWithoutDashSkill = MonsterSkills.Where(x => DashSkill != null && DashSkill.Skill.Id != x.Skill.Id).ToArray();

            foreach (INpcMonsterSkill skill in MonsterSkills)
            {
                Skills.Add(skill);

                if (skill.IsBasicAttack)
                {
                    ReplacedBasicSkill = skill;
                }
            }

            this.InitializeBCards();

            BasicSkill = new SkillInfo
            {
                Vnum = default,
                AttackType = AttackType,
                Range = BasicRange,
                CastTime = BasicCastTime,
                Cooldown = BasicCooldown,
                HitEffect = AttackEffect,
                Element = BaseElement,
                HitChance = builder?.SetHitChance ?? BasicHitChance,
                HitAnimation = 11,
                TargetType = TargetType.Target,
                TargetAffectedEntities = TargetAffectedEntities.Enemies,
                SkillType = SkillType.MonsterSkill,
                Class = 0
            };

            MapInstance = mapInstance;
        }

        public bool IsEnhanced { get; set; }

        public DateTime LastEnhancedEffect { get; set; }

        public bool BaseShouldRespawn { get; }
        public bool TeleportBackOnNoticeRangeExceed { get; }

        public void RefreshStats()
        {
            Speed = this.GetSpeed(BaseSpeed);
        }

        public ConcurrentDictionary<long, int> PlayersDamage { get; } = new();

        public DateTime LastMpRegen { get; set; }

        public Position? GoToBossPosition { get; set; }
        public bool IsInstantBattle { get; set; }

        public IEnumerable<DropChance> RaidDrop { get; }
        public DateTime LastBonusEffectTime { get; set; }
        public DateTime AttentionTime { get; set; }
        public ConcurrentDictionary<byte, Waypoint> Waypoints { get; set; }
        public DateTime LastWayPoint { get; set; }
        public byte CurrentWayPoint { get; set; }
        public bool IgnoreSkillRange { get; set; }
        public byte ReturnTimeOut { get; set; }
        public (VisualType, long) LastAttackedEntity { get; set; }
        public bool IsRunningAway { get; set; }
        public IReadOnlyList<INpcMonsterSkill> NotBasicSkills { get; }
        public IReadOnlyList<INpcMonsterSkill> SkillsWithoutDashSkill { get; }
        public INpcMonsterSkill ReplacedBasicSkill { get; }
        public INpcMonsterSkill DashSkill { get; }

        public IBCardComponent BCardComponent { get; }
        public IChargeComponent ChargeComponent { get; }

        public IAlzanorComponent AlzanorComponent { get; }

        public IBCardStackComponent BCardStackComponent { get; }
        public ThreadSafeHashSet<Guid> AggroedEntities { get; } = new();
        public IBuffComponent BuffComponent { get; }
        public IBCardDataComponent BCardDataComponent { get; }
        
        public bool IsMateTrainer { get; }
        public bool IsSparringMonster { get; }
        public int? FakeMaxHp { get; set; }

        public int MaxHp
        {
            get => this.GetMaxHp((int)(BaseMaxHp * _enhancedHp));
            set => throw new NotImplementedException();
        }

        public int MaxMp
        {
            get => this.GetMaxMp(BaseMaxMp);
            set => throw new NotImplementedException();
        }

        public int Hp { get; set; }
        public int Mp { get; set; }

        public ThreadSafeHashSet<IBattleEntity> Damagers { get; }
        public ThreadSafeHashSet<IBattleEntity> Targets { get; } = new();
        public ThreadSafeHashSet<(VisualType, long)> TargetsByVisualTypeAndId { get; } = new();
        public DateTime LastTargetsRefresh { get; set; }

        public DateTime Death { get; set; }

        public bool IsStillAlive { get; set; }

        public byte Level { get; set; }
        
        public byte HeroLevel { get; set; }

        public byte Element
        {
            get => BaseElement;
            set => throw new NotImplementedException();
        }

        public int ElementRate
        {
            get => BaseElementRate;
            set => throw new NotImplementedException();
        }

        public int FireResistance
        {
            get => BaseFireResistance;
            set => throw new NotImplementedException();
        }

        public int WaterResistance
        {
            get => BaseWaterResistance;
            set => throw new NotImplementedException();
        }

        public int LightResistance
        {
            get => BaseLightResistance;
            set => throw new NotImplementedException();
        }

        public int DarkResistance
        {
            get => BaseDarkResistance;
            set => throw new NotImplementedException();
        }

        public int DamagesMinimum { get; set; }
        public int DamagesMaximum { get; set; }

        public Position Position
        {
            get => new(PositionX, PositionY);
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }

        public short PositionX { get; private set; }

        public short PositionY { get; private set; }

        public FactionType Faction { get; }

        public byte Speed { get; set; }

        public int Size
        {
            get => _size 
                ?? _characterSizeModifiersConfiguration.SizeTypes
                    .First(x => x.EntityType == EntityType.Monster)
                    .DefaultSize;
            set => _size = value;
        }
        public bool IsBonus { get; set; }

        public bool IsBoss { get; }

        public string Name { get; }
        public short NoticeRange { get; }
        public bool OnDefenseOnlyOnce { get; }
        public short PermanentEffect { get; }

        public SummonType? SummonType { get; }
        public bool IsHostile { get; set; }

        public bool IsTarget { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastSkill { get; set; }

        public IMapInstance MapInstance { get; set; }

        public List<IBattleEntitySkill> Skills { get; set; } = new();

        public IBattleEntity Target { get; set; }
        public DateTime NextTick { get; set; }
        public DateTime NextAttackReady { get; set; }
        public bool ModeIsActive { get; set; }
        public short Morph { get; set; }
        public long ModeDeathsSinceRespawn { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public bool VesselMonster { get; set; }
        public bool VesselChristmasMonster { get; set; }
        public short AmountRequired { get; }
        public byte ArmorLevel { get; }
        public AttackType AttackType { get; }
        public byte AttackUpgrade { get; }
        public byte BasicCastTime { get; }
        public short BasicCooldown { get; }
        public byte BasicRange { get; }
        public short AttackEffect { get; }
        public short BasicHitChance { get; }
        public bool CanBeCollected { get; }
        public bool CanBeDebuffed { get; }
        public bool CanBeCaught { get; }
        public bool CanBePushed { get; }
        public bool CanRegenMp { get; }
        public bool CanWalk { get; set; }
        public int CellSize { get; }
        public int CleanDamageMin { get; }
        public int CleanDamageMax { get; }
        public int CleanHitRate { get; }
        public int CleanMeleeDefence { get; }
        public int CleanRangeDefence { get; }
        public int CleanMagicDefence { get; }
        public int CleanDodge { get; }
        public int CleanHp { get; }
        public int CleanMp { get; }
        public short BaseCloseDefence { get; private set; }

        public short BaseConcentrate { get; private set; }

        public short BaseCriticalChance { get; }
        public short BaseCriticalRate { get; }
        public bool DamagedOnlyLastJajamaruSkill { get; }
        public int BaseDamageMaximum { get; private set; }

        public int BaseDamageMinimum { get; private set; }

        public short BaseDarkResistance { get; }
        public short DeathEffect { get; }
        public int BaseMaxHp { get; private set; }

        public int BaseMaxMp { get; private set; }

        public short DefenceDodge { get; private set; }

        public byte DefenceUpgrade { get; }
        public bool DisappearAfterHitting { get; }
        public bool DisappearAfterSeconds { get; }
        public bool DisappearAfterSecondsMana { get; }
        public short DistanceDefence { get; private set; }

        public short DistanceDefenceDodge { get; private set; }

        public byte BaseElement { get; }
        public short BaseElementRate { get; }
        public short BaseFireResistance { get; }
        public FactionType? SuggestedFaction => Faction;
        public int GiveDamagePercentage { get; }
        public int GroupAttack { get; }
        public bool HasMode { get; }
        public int RawHostility { get; }
        public int IconId { get; }
        public bool IsPercent { get; }
        public int JobXp { get; private set; }

        public byte BaseLevel { get; }
        public byte BaseHeroLevel { get; }
        public short BaseLightResistance { get; }
        public short MagicDefence { get; private set; }

        public short MagicMpFactor { get; }
        public short MeleeHpFactor { get; }
        public sbyte MinimumAttackRange { get; }
        public byte Stars { get; }
        public long? SummonerId { get; set; }
        public VisualType? SummonerType { get; set; }

        public IBattleEntity Killer { get; set; }

        public Guid UniqueId { get; }

        public void GenerateDeath(IBattleEntity killer = null)
        {
            IsStillAlive = false;
            Hp = 0;
            Mp = 0;
            Death = DateTime.UtcNow;
            this.RemoveAllBuffsAsync(true);
            Target = null;
        }

        public VisualType Type => VisualType.Monster;
        public int Id { get; set; }
        public byte Direction { get; set; } = 2;
        public byte HitsReceived { get; set; } = 0;
        public sbyte NoDamageChance { get; set; } = 0;

        bool INpcMonsterEntity.ShouldRespawn => BaseShouldRespawn;
        public bool ReturningToFirstPosition { get; set; }
        public bool ShouldFindNewTarget { get; set; }
        public bool FindNewPositionAroundTarget { get; set; }
        public bool IsApproachingTarget { get; set; }
        public bool OnFirstDamageReceive { get; set; }

        public DateTime SpawnDate { get; set; }
        public DateTime LastSpecialHpDecrease { get; set; }

        public bool IsMoving => CanWalk;

        public int MonsterVNum { get; }

        public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false) => _eventContainer.AddEvent(key, notification, removedOnTrigger);

        public async Task TriggerEvents(string key) => await _eventContainer.TriggerEvents(key);

        public async Task EmitEventAsync<T>(T eventArgs) where T : IBattleEntityEvent
        {
            if (eventArgs.Entity != this)
            {
                throw new ArgumentException("An event should be emitted only from the event sender");
            }

            await _asyncEventPipeline.ProcessEventAsync(eventArgs);
        }

        public void EmitEvent<T>(T eventArgs) where T : IBattleEntityEvent
        {
            EmitEventAsync(eventArgs).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public MonsterRaceType MonsterRaceType { get; set; }
        public byte MonsterRaceSubType { get; }
        public short RangeDodgeFactor { get; }
        public TimeSpan BaseRespawnTime { get; }
        public int SpawnMobOrColor { get; }
        public byte BaseSpeed { get; }
        public int SpriteSize { get; }
        public int TakeDamages { get; }
        public short VNumRequired { get; }
        public short BaseWaterResistance { get; }
        public byte WeaponLevel { get; }
        public byte WinfoValue { get; }
        public int Xp { get; private set; }

        public byte MaxTries { get; }
        public short CollectionCooldown { get; }
        public byte CollectionDanceTime { get; }
        public bool TeleportRemoveFromInventory { get; }
        public short BasicDashSpeed { get; }
        public bool ModeIsHpTriggered { get; }
        public byte ModeLimiterType { get; }
        public short ModeRangeTreshold { get; }
        public short ModeCModeVnum { get; }
        public short ModeHpTresholdOrItemVnum { get; }
        public short MidgardDamage { get; }
        public bool HasDash { get; }
        public bool DropToInventory { get; }
        public int BaseXp { get; }
        public int BaseJobXp { get; }
        public IReadOnlyList<DropDTO> Drops { get; }
        public bool CanSeeInvisible { get; }
        public IReadOnlyList<BCardDTO> BCards { get; }
        public IReadOnlyList<BCardDTO> ModeBCards { get; }

        public IReadOnlyList<INpcMonsterSkill> MonsterSkills { get; }

        public SkillInfo BasicSkill { get; }

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
        public bool IsJumping { get; set; }
        
        public bool IsTotem { get; set; }
        public bool HasSpawnRedCircle { get; set; }
        public DateTime LastJump { get; set; }
        public short? SkillToUse { get; set; }
        public bool ForceUseSkill { get; set; }
        public void AddEndBuff(short buffVnum, int damage)
        {
            _endBuffDamageComponent.AddEndBuff(buffVnum, damage);
        }

        public int DecreaseDamageEndBuff(short buffVnum, int damage) => _endBuffDamageComponent.DecreaseDamageEndBuff(buffVnum, damage);

        public void RemoveEndBuffDamage(short buffVnum)
        {
            _endBuffDamageComponent.RemoveEndBuffDamage(buffVnum);
        }

        public void ChangeHpMultiplier(double hp = 1)
        {
            _enhancedHp = hp;
        }

        private void UpdateMonstersBaseStatistics()
        {
            BaseMaxHp = _battleEntityAlgorithmService.GetBasicHp((short)MonsterRaceType, Level, MeleeHpFactor, CleanHp);
            BaseMaxMp = _battleEntityAlgorithmService.GetBasicMp((short)MonsterRaceType, Level, MagicMpFactor, CleanMp);

            BaseDamageMinimum = _battleEntityAlgorithmService.GetAttack(true, (short)MonsterRaceType, AttackType, WeaponLevel, WinfoValue, Level, GetModifier(), CleanDamageMin);
            BaseDamageMaximum = _battleEntityAlgorithmService.GetAttack(false, (short)MonsterRaceType, AttackType, WeaponLevel, WinfoValue, Level, GetModifier(), CleanDamageMax);
            BaseConcentrate = (short)_battleEntityAlgorithmService.GetHitrate((short)MonsterRaceType, AttackType, WeaponLevel, Level, GetModifier(), CleanHitRate);
            BaseCloseDefence = (short)_battleEntityAlgorithmService.GetDefense((short)MonsterRaceType, AttackType.Melee, ArmorLevel, Level, GetModifier(), CleanMeleeDefence);
            DistanceDefence = (short)_battleEntityAlgorithmService.GetDefense((short)MonsterRaceType, AttackType.Ranged, ArmorLevel, Level, GetModifier(), CleanRangeDefence);
            MagicDefence = (short)_battleEntityAlgorithmService.GetDefense((short)MonsterRaceType, AttackType.Magical, ArmorLevel, Level, GetModifier(), CleanMagicDefence);
            DefenceDodge = (short)_battleEntityAlgorithmService.GetDodge((short)MonsterRaceType, ArmorLevel, Level, GetModifier(), CleanDodge);
            DistanceDefenceDodge = (short)_battleEntityAlgorithmService.GetDodge((short)MonsterRaceType, ArmorLevel, Level, GetModifier(), CleanDodge);

            Xp = Level < 20 ? 60 * Level + BaseXp : 70 * Level + BaseXp;
            JobXp = Level > 60 ? 105 + BaseJobXp : 120 + BaseJobXp;

            if (Xp < 0)
            {
                Xp = 0;
            }

            if (JobXp < 0)
            {
                JobXp = 0;
            }
        }

        private int GetModifier()
        {
            return AttackType switch
            {
                AttackType.Melee => MeleeHpFactor,
                AttackType.Ranged => RangeDodgeFactor,
                AttackType.Magical => MagicMpFactor,
                _ => 0
            };
        }
    }
}