// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Maps;

public class MapNpcDTO : IIntDto
{
    public short Dialog { get; set; }
    public int? QuestDialog { get; set; }
    public short Effect { get; set; }
    public short EffectDelay { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsMoving { get; set; }
    public bool IsSitting { get; set; }
    public int MapId { get; set; }

    public byte Faction { get; set; }
    public short MapX { get; set; }
    public short MapY { get; set; }
    public int NpcVNum { get; set; }
    public byte Direction { get; set; }
    public bool CanAttack { get; set; }
    public bool HasGodMode { get; set; }
    public string CustomName { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
}