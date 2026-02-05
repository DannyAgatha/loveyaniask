// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Skills;

public class NpcMonsterSkillDTO : IIntDto
{
    public int NpcMonsterVNum { get; set; }

    public short Rate { get; set; }

    public short SkillVNum { get; set; }

    /// <summary>
    ///     Tells whether or not the skill is considered as a basic attack
    /// </summary>
    public bool IsBasicAttack { get; set; }

    public bool IsIgnoringHitChance { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}