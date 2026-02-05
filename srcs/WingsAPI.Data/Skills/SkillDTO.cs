// NosEmu
// 


using System.Collections.Generic;
using PhoenixLib.DAL;
using WingsEmu.DTOs.BCards;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.DTOs.Skills;

public class SkillDTO : IIntDto
{
    public short DashSpeed { get; set; }

    public short SuAnimation { get; set; }

    public short CtAnimation { get; set; }

    public short SuEffect { get; set; }

    public short CtEffect { get; set; }

    public short CastId { get; set; }

    public short CastTime { get; set; }

    public byte Class { get; set; }

    public short Cooldown { get; set; }

    public byte CPCost { get; set; }

    public short Duration { get; set; }

    public byte Element { get; set; }

    public TargetHitType HitType { get; set; }

    public short ItemVNum { get; set; }

    public byte Level { get; set; }

    public byte LevelMinimum { get; set; }

    public byte MinimumAdventurerLevel { get; set; }

    public byte MinimumArcherLevel { get; set; }

    public byte MinimumMagicianLevel { get; set; }

    public byte MinimumSwordmanLevel { get; set; }

    public short MpCost { get; set; }

    public string Name { get; set; }

    public int Price { get; set; }

    public byte Range { get; set; }

    public SkillType SkillType { get; set; }

    public short AoERange { get; set; }

    public TargetType TargetType { get; set; }

    public AttackType AttackType { get; set; }

    /// <summary>
    ///     If <see cref="SkillType" /> == 2 (SkillUpgrade) - UpgradeSkill = Parent UpgradeSkill Skill Vnum
    /// </summary>
    public short UpgradeSkill { get; set; }

    public short UpgradeType { get; set; }

    public int SpecialCost { get; set; }

    public TargetAffectedEntities TargetAffectedEntities { get; set; }

    public bool IsUsingSecondWeapon { get; set; }
    public List<BCardDTO> BCards { get; set; } = new();
    public List<ComboDTO> Combos { get; set; } = new();
    public int Id { get; set; }
}