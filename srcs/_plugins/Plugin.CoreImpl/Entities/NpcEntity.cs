// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Drops;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace Plugin.CoreImpl.Entities
{
    public class NpcEntity : INpcEntity
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly ICastingComponent _castingComponent;
        private readonly IEndBuffDamageComponent _endBuffDamageComponent;
        private readonly IEventTriggerContainer _eventContainer;
        private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;
        
        private int? _size;
        public NpcEntity(IEventTriggerContainer eventContainer, IBCardComponent bCardComponent, IAsyncEventPipeline asyncEventPipeline, IMonsterData monsterData, IMapInstance mapInstance,
            ISkillsManager skillsManager, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration,
            MapNpcDTO npcDto = null, ShopNpc shopNpc = null, int? id = null, INpcAdditionalData npcAdditionalData = null, short? optionalDialog = null)
        {
            UniqueId = Guid.NewGuid();
            BCardComponent = bCardComponent;
            _castingComponent = new CastingComponent();
            ChargeComponent = new ChargeComponent();
            AlzanorComponent = new AlzanorComponent();
            BCardStackComponent = new BCardStackComponent();
            Damagers = new ThreadSafeHashSet<IBattleEntity>();
            _endBuffDamageComponent = new EndBuffDamageComponent();
            _eventContainer = eventContainer;
            BuffComponent = new BuffComponent();
            BCardDataComponent = new BCardDataComponent();
            _asyncEventPipeline = asyncEventPipeline;
            _characterSizeModifiersConfiguration  = characterSizeModifiersConfiguration;
            
            ShouldRespawn = true;

            IsHostile = monsterData.RawHostility != (int)HostilityType.NOT_HOSTILE;
            if (npcDto != null)
            {
                Id = npcDto.Id;
                Dialog = npcDto.Dialog;
                QuestDialog = npcDto.QuestDialog;
                Direction = npcDto.Direction;
                Effect = npcDto.Effect;
                EffectDelay = TimeSpan.FromMilliseconds(npcDto.EffectDelay);
                IsDisabled = npcDto.IsDisabled;
                IsMoving = npcDto.IsMoving;
                IsSitting = npcDto.IsSitting;
                PositionX = npcDto.MapX;
                PositionY = npcDto.MapY;
                MapId = npcDto.MapId;
                CanAttack = npcDto.CanAttack;
                HasGodMode = npcDto.HasGodMode;
                CustomName = npcDto.CustomName;
                IsHostile = npcDto.CanAttack;
                Faction = (FactionType)npcDto.Faction;
            }

            Dialog = optionalDialog ?? Dialog;
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
            CanWalk = monsterData.CanWalk;
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
            BaseDamageMaximum = monsterData.BaseDamageMaximum;
            BaseDamageMinimum = monsterData.BaseDamageMinimum;
            BaseDarkResistance = monsterData.BaseDarkResistance;
            DeathEffect = monsterData.DeathEffect;
            BaseMaxHp = monsterData.BaseMaxHp;
            BaseMaxMp = monsterData.BaseMaxMp;
            DefenceDodge = monsterData.DefenceDodge;
            DefenceUpgrade = monsterData.DefenceUpgrade;
            DisappearAfterHitting = monsterData.DisappearAfterHitting;
            DisappearAfterSeconds = monsterData.DisappearAfterSeconds;
            DisappearAfterSecondsMana = monsterData.DisappearAfterSecondsMana;
            DistanceDefence = monsterData.DistanceDefence;
            DistanceDefenceDodge = monsterData.DistanceDefenceDodge;
            Drops = monsterData.Drops;
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
            Stars = monsterData.Stars;
            Level = BaseLevel;
            HeroLevel = BaseHeroLevel;

            if (npcAdditionalData != null)
            {
                IsTimeSpaceMate = npcAdditionalData.IsTimeSpaceMate;
                IsProtected = npcAdditionalData.IsProtected;
                MinilandOwner = npcAdditionalData.MinilandOwner;
                ShouldRespawn = npcAdditionalData.NpcShouldRespawn;
                CanMove = npcAdditionalData.CanMove;
                IsMoving = npcAdditionalData.CanMove;
                CanAttack = npcAdditionalData.CanAttack;
                Direction = npcAdditionalData.NpcDirection;
                IsHostile = npcAdditionalData.IsHostile;
                Faction = npcAdditionalData.FactionType;
                TimeSpaceInfo = npcAdditionalData.TimeSpaceInfo;
                TimeSpaceOwnerId = npcAdditionalData.TimeSpaceOwnerId;
                RainbowFlag = npcAdditionalData.RainbowFlag;

                if (npcAdditionalData.HpMultiplier.HasValue)
                {
                    BaseMaxHp = (int)(BaseMaxHp * npcAdditionalData.HpMultiplier.Value);
                    Hp = BaseMaxHp;
                }

                if (npcAdditionalData.MpMultiplier.HasValue)
                {
                    BaseMaxMp = (int)(BaseMaxMp * npcAdditionalData.MpMultiplier.Value);
                    Mp = BaseMaxMp;
                }

                if (npcAdditionalData.CustomLevel.HasValue)
                {
                    Level = npcAdditionalData.CustomLevel.Value;
                }
            }

            LastEffect = Death = SpawnDate = DateTime.UtcNow;
            FirstX = PositionX;
            FirstY = PositionY;
            IsStillAlive = true;
            Speed = BaseSpeed;
            Hp = BaseMaxHp;
            Mp = BaseMaxMp;
            Killer = null;
            Target = null;
            MonsterSkills = monsterData.MonsterSkills;
            this.InitializeBCards();
            ShopNpc = shopNpc;
            CurrentCollection = MaxTries;
            CanSeeInvisible = monsterData.CanSeeInvisible;

            // pure puke, needs proper entity rework :(
            if (monsterData is NpcMonsterDto npcMonsterDto)
            {
                var skills = new List<INpcMonsterSkill>();

                foreach (NpcMonsterSkillDTO skill in npcMonsterDto.Skills)
                {
                    var monsterSkill = new NpcMonsterSkill(skillsManager.GetSkill(skill.SkillVNum), skill.Rate, skill.IsBasicAttack, skill.IsIgnoringHitChance);
                    skills.Add(monsterSkill);
                    Skills.Add(monsterSkill);

                    if (monsterSkill.IsBasicAttack)
                    {
                        ReplacedBasicSkill = monsterSkill;
                    }
                }

                NotBasicSkills = skills.Where(x => !x.IsBasicAttack).ToArray();
                if (HasDash)
                {
                    DashSkill = skills.Count != 0 ? skills[0] : null;
                }

                SkillsWithoutDashSkill = skills.Where(x => DashSkill != null && DashSkill.Skill.Id != x.Skill.Id).ToArray();
            }

            BasicSkill = new SkillInfo
            {
                Vnum = default,
                AttackType = AttackType,
                Range = BasicRange,
                CastTime = BasicCastTime,
                Cooldown = BasicCooldown,
                HitEffect = AttackEffect,
                Element = BaseElement,
                HitChance = BasicHitChance,
                HitAnimation = 11,
                TargetType = TargetType.Target,
                TargetAffectedEntities = TargetAffectedEntities.Enemies,
                SkillType = SkillType.MonsterSkill,
                Class = 0
            };

            if (id != null)
            {
                Id = id.Value;
            }

            if (Id == default)
            {
                Id = mapInstance.GenerateEntityId();
            }

            MapInstance = mapInstance;
        }

        public IBCardComponent BCardComponent { get; }
        public IChargeComponent ChargeComponent { get; }

        public IAlzanorComponent AlzanorComponent { get; }
        public IBCardStackComponent BCardStackComponent { get; }
        public IBuffComponent BuffComponent { get; }
        public IBCardDataComponent BCardDataComponent { get; }

        public int Size
        {
            get => _size ?? _characterSizeModifiersConfiguration.SizeTypes
                .First(static x => x.EntityType == EntityType.NPC)
                .DefaultSize;
            set => _size = value;
        }
        public DateTime SpawnDate { get; set; }

        public VisualType Type => VisualType.Npc;
        
        public byte Stars { get; }

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

        #region Properties

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
        public bool CanWalk { get; }
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
        public short BaseCloseDefence { get; }
        public short BaseConcentrate { get; }
        public short BaseCriticalChance { get; }
        public short BaseCriticalRate { get; }
        public bool DamagedOnlyLastJajamaruSkill { get; }
        public int BaseDamageMaximum { get; }
        public int BaseDamageMinimum { get; }
        public short BaseDarkResistance { get; }
        public short DeathEffect { get; }
        public int BaseMaxHp { get; }
        public int BaseMaxMp { get; }
        public short DefenceDodge { get; }
        public byte DefenceUpgrade { get; }
        public bool DisappearAfterHitting { get; }
        public bool DisappearAfterSeconds { get; }
        public bool DisappearAfterSecondsMana { get; }
        public short DistanceDefence { get; }
        public short DistanceDefenceDodge { get; }
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
        public int JobXp { get; }
        public byte BaseLevel { get; }
        public byte BaseHeroLevel { get; }
        public short BaseLightResistance { get; }
        public short MagicDefence { get; }
        public short MagicMpFactor { get; }
        public short MeleeHpFactor { get; }
        public sbyte MinimumAttackRange { get; }
        public int MonsterVNum { get; }
        public string Name { get; }
        public short NoticeRange { get; }
        public bool OnDefenseOnlyOnce { get; }
        public short PermanentEffect { get; }
        public MonsterRaceType MonsterRaceType { get; }
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
        public int Xp { get; }
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
        public bool TeleportBackOnNoticeRangeExceed { get; }
        public int BaseXp { get; }
        public int BaseJobXp { get; }

        public FactionType Faction { get; } = FactionType.Neutral;

        public byte Speed { get; set; }
        public bool ShouldRespawn { get; set; }
        public short FirstX { get; set; }
        public short FirstY { get; set; }
        public Guid UniqueId { get; }
        public bool IsHostile { get; }
        public float? HpMultiplier { get; }
        public float? MpMultiplier { get; }
        public byte? CustomLevel { get; }
        public FactionType FactionType { get; }
        public long? TimeSpaceOwnerId { get; }
        public TimeSpaceFileConfiguration TimeSpaceInfo { get; }
        public bool IsTimeSpaceMate { get; set; }
        public bool IsProtected { get; set; }
        public IPlayerEntity MinilandOwner { get; }
        public bool NpcShouldRespawn { get; }
        public bool CanMove { get; }
        public ThreadSafeHashSet<IBattleEntity> Targets { get; } = new();
        public ThreadSafeHashSet<(VisualType, long)> TargetsByVisualTypeAndId { get; } = new();
        public DateTime LastTargetsRefresh { get; set; }
        public DateTime Death { get; set; }
        public DateTime LastEffect { get; set; }
        public DateTime LastSkill { get; set; }
        public DateTime LastSpecialHpDecrease { get; set; }

        public IMapInstance MapInstance { get; private set; }

        public IBattleEntity Killer { get; set; }
        public IBattleEntity Target { get; set; }
        public DateTime NextTick { get; set; }
        public DateTime NextAttackReady { get; set; }
        public bool ModeIsActive { get; set; }
        public short Morph { get; set; }
        public long ModeDeathsSinceRespawn { get; set; }
        public (VisualType, long) LastAttackedEntity { get; set; }
        public bool IsRunningAway { get; set; }
        public byte ReturnTimeOut { get; set; }
        public IReadOnlyList<INpcMonsterSkill> NotBasicSkills { get; }
        public IReadOnlyList<INpcMonsterSkill> SkillsWithoutDashSkill { get; }
        public INpcMonsterSkill ReplacedBasicSkill { get; }
        public INpcMonsterSkill DashSkill { get; }

        public byte Level { get; set; }
        
        public byte HeroLevel { get; set; }
        public int Hp { get; set; }

        public int MaxHp
        {
            get => this.GetMaxHp(BaseMaxHp);
            set => throw new NotImplementedException();
        }

        public int Mp { get; set; }

        public int MaxMp
        {
            get => this.GetMaxMp(BaseMaxMp);
            set => throw new NotImplementedException();
        }

        public SkillInfo BasicSkill { get; }
        public ThreadSafeHashSet<IBattleEntity> Damagers { get; }

        public IReadOnlyList<DropDTO> Drops { get; }

        public bool CanSeeInvisible { get; }

        public IReadOnlyList<BCardDTO> BCards { get; }
        public IReadOnlyList<BCardDTO> ModeBCards { get; }

        private IReadOnlyList<INpcMonsterSkill> _monsterSkills;

        public IReadOnlyList<INpcMonsterSkill> MonsterSkills
        {
            get => _monsterSkills;
            private set
            {
                _monsterSkills = value;
                Skills = value.Cast<IBattleEntitySkill>().ToList();
            }
        }

        public List<IBattleEntitySkill> Skills { get; private set; }

        public byte Element { get; set; }
        public int ElementRate { get; set; }
        public int FireResistance { get; set; }
        public int WaterResistance { get; set; }
        public int LightResistance { get; set; }
        public int DarkResistance { get; set; }
        public int DamagesMinimum { get; set; }
        public int DamagesMaximum { get; set; }

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

        public short PositionX { get; private set; }
        public short PositionY { get; private set; }
        public bool IsStillAlive { get; set; }
        public bool ReturningToFirstPosition { get; set; }
        public bool ShouldFindNewTarget { get; set; }
        public bool FindNewPositionAroundTarget { get; set; }
        public bool IsApproachingTarget { get; set; }
        public bool OnFirstDamageReceive { get; set; }
        public bool HasGodMode { get; }
        public byte CurrentCollection { get; set; }
        public DateTime LastCollection { get; set; } = DateTime.UtcNow;
        public string CustomName { get; }
        public long? CharacterPartnerId { get; set; }
        public DateTime LastBasicAttack { get; set; }
        public DateTime LastTimeSpaceHeal { get; set; }
        public DateTime LastFrozenEffect { get; set; }
        public RainBowFlag RainbowFlag { get; set; }

        public void ChangeMapInstance(IMapInstance mapInstance)
        {
            MapInstance = mapInstance;
        }

        public bool CanAttack { get; set; }
        public byte NpcDirection { get; }

        public ThreadSafeHashSet<Guid> AggroedEntities { get; } = new();

        public int Id { get; }
        public short Dialog { get; }
        public int? QuestDialog { get; }
        public short Effect { get; }
        public TimeSpan EffectDelay { get; }
        public bool IsDisabled { get; }
        public bool IsMoving { get; }
        public bool IsSitting { get; }
        public int MapId { get; }
        public int NpcVNum => MonsterVNum;
        public ShopNpc ShopNpc { get; set; }
        public byte Direction { get; set; }
        public byte HitsReceived { get; set; }
        public sbyte NoDamageChance { get; set; }
        public byte BaseStarLevelTaming { get; set; }

        #endregion
    }
}