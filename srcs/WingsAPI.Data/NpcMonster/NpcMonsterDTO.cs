// NosEmu
// 


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using WingsAPI.Data.Drops;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.DTOs.NpcMonster;

public class NpcMonsterDto : IIntDto
{
    public byte BaseStarLevelTaming { get; set; }
    public short AmountRequired { get; set; }

    public AttackType AttackType { get; set; }

    public byte AttackUpgrade { get; set; }

    public byte BasicCastTime { get; set; }

    public short BasicCooldown { get; set; }

    public byte BasicRange { get; set; }

    public short BasicDashSpeed { get; set; }

    public short AttackEffect { get; set; }

    public short PermanentEffect { get; set; }

    public short DeathEffect { get; set; }

    public short BasicHitChance { get; set; }

    public short CloseDefence { get; set; }

    public short Concentrate { get; set; }

    public short CriticalChance { get; set; }

    public short CriticalRate { get; set; }

    public int DamageMaximum { get; set; }

    public int DamageMinimum { get; set; }

    public short DarkResistance { get; set; }

    public short DefenceDodge { get; set; }

    public byte DefenceUpgrade { get; set; }

    public short DistanceDefence { get; set; }

    public short DistanceDefenceDodge { get; set; }

    public byte Element { get; set; }

    public short ElementRate { get; set; }

    public short FireResistance { get; set; }

    public int HostilityType { get; set; }

    public int GroupAttack { get; set; }

    public int JobXp { get; set; }

    public byte Level { get; set; }
    
    public byte HeroLevel { get; set; }
    public short LightResistance { get; set; }

    public short MagicDefence { get; set; }

    public int MaxHp { get; set; }

    public int MaxMp { get; set; }

    public string Name { get; set; }

    public short NoticeRange { get; set; }

    public byte Race { get; set; }

    public byte RaceType { get; set; }

    public int RespawnTime { get; set; }

    public byte Speed { get; set; }

    public short VNumRequired { get; set; }

    public short WaterResistance { get; set; }

    public int Xp { get; set; }

    public bool IsPercent { get; set; }

    public int TakeDamages { get; set; }

    public int GiveDamagePercentage { get; set; }

    public int IconId { get; set; }

    public int SpawnMobOrColor { get; set; }

    public int SpriteSize { get; set; }

    public int CellSize { get; set; }

    public short MeleeHpFactor { get; set; }

    public short RangeDodgeFactor { get; set; }

    public short MagicMpFactor { get; set; }

    public bool CanWalk { get; set; }

    public bool CanBeCollected { get; set; }

    public bool CanBeDebuffed { get; set; }

    public bool CanBeCaught { get; set; }

    public bool DisappearAfterSeconds { get; set; }

    public bool DisappearAfterHitting { get; set; }

    public bool HasMode { get; set; }

    public bool DisappearAfterSecondsMana { get; set; }

    public bool OnDefenseOnlyOnce { get; set; }

    public bool CanRegenMp { get; set; }

    public bool CanBePushed { get; set; }

    public bool DamagedOnlyLastJajamaruSkill { get; set; }

    public sbyte MinimumAttackRange { get; set; }

    public byte WeaponLevel { get; set; }

    public byte ArmorLevel { get; set; }

    public byte WinfoValue { get; set; }

    public int CleanDamageMin { get; set; }

    public int CleanDamageMax { get; set; }

    public int CleanHitRate { get; set; }

    public int CleanMeleeDefence { get; set; }

    public int CleanRangeDefence { get; set; }

    public int CleanMagicDefence { get; set; }

    public int CleanDodge { get; set; }

    public int CleanHp { get; set; }

    public int CleanMp { get; set; }

    public byte MaxTries { get; set; }

    public short CollectionCooldown { get; set; }

    public byte CollectionDanceTime { get; set; }

    public bool TeleportRemoveFromInventory { get; set; }

    public bool ModeIsHpTriggered { get; set; }

    public byte ModeLimiterType { get; set; }

    public short ModeRangeTreshold { get; set; }

    public short ModeCModeVnum { get; set; }

    public short ModeHpTresholdOrItemVnum { get; set; }

    public short MidgardDamage { get; set; }

    public bool HasDash { get; set; }

    public int BaseXp { get; set; }

    public int BaseJobXp { get; set; }
    
    public byte Stars { get; set; }

    public bool DropToInventory { get; set; }
    public List<DropDTO> Drops { get; set; } = new();
    public List<NpcMonsterSkillDTO> Skills { get; set; } = new();
    public List<BCardDTO> BCards { get; set; } = new();
    public List<BCardDTO> ModeBCards { get; set; } = new();

    /// <summary>
    ///     VNUM in client files
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
}