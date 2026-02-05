// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Skills;

public class ComboDTO : IIntDto
{
    public short Animation { get; set; }
    public short Effect { get; set; }
    public short Hit { get; set; }
    public int SkillVNum { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
}