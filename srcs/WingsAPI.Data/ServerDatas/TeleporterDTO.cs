// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using WingsEmu.Packets.Enums;

namespace WingsEmu.DTOs.ServerDatas;

public class TeleporterDTO : IIntDto
{
    public short Index { get; set; }

    public TeleporterType Type { get; set; }

    public int MapId { get; set; }

    public int MapNpcId { get; set; }

    public short MapX { get; set; }

    public short MapY { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}