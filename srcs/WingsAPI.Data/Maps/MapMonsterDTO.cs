// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Maps;

public class MapMonsterDTO : IIntDto
{
    public bool IsMoving { get; set; }

    public int MapId { get; set; }

    public short MapX { get; set; }

    public short MapY { get; set; }

    public int MonsterVNum { get; set; }
    public byte Direction { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}