using System;
using System.Collections.Generic;
using WingsAPI.Data.Drops;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Entities;

public interface IMonsterData
{
    short AmountRequired { get; }
    byte ArmorLevel { get; }
    AttackType AttackType { get; }
    byte AttackUpgrade { get; }
    byte BasicCastTime { get; }
    short BasicCooldown { get; }
    byte BasicRange { get; }
    short AttackEffect { get; }
    short BasicHitChance { get; }
    bool CanBeCollected { get; }
    bool CanBeDebuffed { get; }
    bool CanBeCaught { get; }
    bool CanBePushed { get; }
    bool CanRegenMp { get; }
    bool CanWalk { get; }
    int CellSize { get; }
    int CleanDamageMin { get; }
    int CleanDamageMax { get; }
    int CleanHitRate { get; }
    int CleanMeleeDefence { get; }
    int CleanRangeDefence { get; }
    int CleanMagicDefence { get; }
    int CleanDodge { get; }
    int CleanHp { get; }
    int CleanMp { get; }
    short BaseCloseDefence { get; }
    short BaseConcentrate { get; }
    short BaseCriticalChance { get; }
    short BaseCriticalRate { get; }
    bool DamagedOnlyLastJajamaruSkill { get; }
    int BaseDamageMaximum { get; }
    int BaseDamageMinimum { get; }
    short BaseDarkResistance { get; }
    short DeathEffect { get; }
    int BaseMaxHp { get; }
    int BaseMaxMp { get; }
    short DefenceDodge { get; }
    byte DefenceUpgrade { get; }
    bool DisappearAfterHitting { get; }
    bool DisappearAfterSeconds { get; }
    bool DisappearAfterSecondsMana { get; }
    short DistanceDefence { get; }
    short DistanceDefenceDodge { get; }
    byte BaseElement { get; }
    short BaseElementRate { get; }
    short BaseFireResistance { get; }
    FactionType? SuggestedFaction { get; }
    int GiveDamagePercentage { get; }
    int GroupAttack { get; }
    bool HasMode { get; }
    int RawHostility { get; }
    int IconId { get; }
    bool IsPercent { get; }
    int JobXp { get; }
    byte BaseLevel { get; }
    byte BaseHeroLevel { get; }
    short BaseLightResistance { get; }
    short MagicDefence { get; }
    short MagicMpFactor { get; }
    short MeleeHpFactor { get; }
    sbyte MinimumAttackRange { get; }
    int MonsterVNum { get; }
    string Name { get; }
    short NoticeRange { get; }
    bool OnDefenseOnlyOnce { get; }
    short PermanentEffect { get; }
    MonsterRaceType MonsterRaceType { get; }
    byte MonsterRaceSubType { get; }
    short RangeDodgeFactor { get; }
    TimeSpan BaseRespawnTime { get; }
    int SpawnMobOrColor { get; }
    byte BaseSpeed { get; }
    int SpriteSize { get; }
    int TakeDamages { get; }
    short VNumRequired { get; }
    short BaseWaterResistance { get; }
    byte WeaponLevel { get; }
    byte WinfoValue { get; }
    int Xp { get; }
    byte MaxTries { get; }
    short CollectionCooldown { get; }
    byte CollectionDanceTime { get; }
    bool TeleportRemoveFromInventory { get; }
    short BasicDashSpeed { get; }
    bool ModeIsHpTriggered { get; }
    byte ModeLimiterType { get; }
    short ModeRangeTreshold { get; }
    short ModeCModeVnum { get; }
    short ModeHpTresholdOrItemVnum { get; }
    short MidgardDamage { get; }
    bool HasDash { get; }
    bool DropToInventory { get; }
    int BaseXp { get; }
    int BaseJobXp { get; }
    byte Stars { get; }
    public IReadOnlyList<DropDTO> Drops { get; }
    public bool CanSeeInvisible { get; }
    public IReadOnlyList<BCardDTO> BCards { get; }
    public IReadOnlyList<BCardDTO> ModeBCards { get; }
    public IReadOnlyList<INpcMonsterSkill> MonsterSkills { get; }
}