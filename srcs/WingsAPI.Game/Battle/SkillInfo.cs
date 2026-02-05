using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Battle;

public class SkillInfo
{
    public short Class { get; set; }
    public SkillType SkillType { get; set; }
    public int Vnum { get; set; }
    public AttackType AttackType { get; set; }
    public List<BCardDTO> BCards { get; set; } = new();
    public short Element { get; set; }

    public short CastAnimation { get; set; }
    public short CastEffect { get; set; }
    public short CastId { get; set; }

    public short HitAnimation { get; set; }
    public short HitEffect { get; set; }

    public byte Range { get; set; }
    public short AoERange { get; set; }

    public short Cooldown { get; set; }
    public short CastTime { get; set; }
    public TargetType TargetType { get; set; }
    public TargetHitType HitType { get; set; }
    public List<ComboDTO> Combos { get; set; } = new();
    public IReadOnlyDictionary<SkillCastType, HashSet<BCardDTO>> BCardsType { get; set; } = new Dictionary<SkillCastType, HashSet<BCardDTO>>();
    public TargetAffectedEntities TargetAffectedEntities { get; set; }
    public short HitChance { get; set; }
    public bool IsUsingSecondWeapon { get; set; }
    public bool IsComboSkill { get; set; }
    public int ManaCost { get; set; }
    public int? PartnerSkillRank { get; set; }
    
    public bool IsCanceled { get; set; }
}