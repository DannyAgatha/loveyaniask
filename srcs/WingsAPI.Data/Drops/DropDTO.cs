// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Drops;

public class DropDTO : IIntDto
{
    public int Amount { get; set; }

    public int DropChance { get; set; }

    public int ItemVNum { get; set; }
    public int? MapId { get; set; }
    public int? MonsterVNum { get; set; }
    public int? RaceType { get; set; }
    public int? RaceSubType { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}