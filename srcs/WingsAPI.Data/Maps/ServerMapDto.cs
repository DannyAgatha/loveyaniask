using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Maps;

public class ServerMapDto : IIntDto
{
    public int MapVnum { get; set; }

    public int NameId { get; set; }

    public int MusicId { get; set; }

    public int AmbientId { get; set; }
    
    public bool IsBossRoom { get; set; }

    public List<MapFlags> Flags { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
}