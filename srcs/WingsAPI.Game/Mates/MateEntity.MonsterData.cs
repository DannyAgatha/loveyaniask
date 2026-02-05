using System;
using System.Collections.Generic;
using WingsAPI.Data.Drops;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Mates;

public partial class MateEntity
{
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
    public byte DefenceUpgrade { get; }
    public bool DisappearAfterHitting { get; }
    public bool DisappearAfterSeconds { get; }
    public bool DisappearAfterSecondsMana { get; }
    public short DistanceDefenceDodge { get; }
    public byte BaseElement { get; }
    public short BaseElementRate { get; }
    public short BaseFireResistance { get; }
    public FactionType? SuggestedFaction { get; }
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
    public IReadOnlyList<DropDTO> Drops { get; }
    public bool CanSeeInvisible { get; }
    public IReadOnlyList<BCardDTO> BCards { get; }
    public IReadOnlyList<BCardDTO> ModeBCards { get; }
    public IReadOnlyList<INpcMonsterSkill> MonsterSkills { get; }
}