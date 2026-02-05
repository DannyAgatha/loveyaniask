// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.ServerDatas;

public class PortalDTO : IIntDto
{
    public int DestinationMapId { get; set; }

    public short DestinationX { get; set; }

    public short DestinationY { get; set; }

    public bool IsDisabled { get; set; }

    public int SourceMapId { get; set; }

    public short SourceX { get; set; }

    public short SourceY { get; set; }

    public short Type { get; set; }

    public short? RaidType { get; set; }

    public short? MapNameId { get; set; }
    
    public short? LevelRequired { get; set; }
    
    public short? HeroLevelRequired { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}