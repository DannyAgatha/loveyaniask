using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using WingsEmu.Packets.Enums;

namespace WingsEmu.DTOs.Quests;

public class QuestDto : IIntDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool AutoFinish { get; set; }
    public int DialogStarting { get; set; }
    public int DialogFinish { get; set; }
    public int DialogDuring { get; set; }
    public byte MinLevel { get; set; }
    public byte MaxLevel { get; set; }
    public int NextQuestId { get; set; }
    public QuestType QuestType { get; set; }
    public int RequiredQuestId { get; set; }
    public int TalkerVnum { get; set; }
    public short TargetMapId { get; set; }
    public short TargetMapX { get; set; }
    public short TargetMapY { get; set; }
    public int Unknown1 { get; set; }
    public bool IsBlue { get; set; }
    public List<QuestPrizeDto> Prizes { get; set; } = new();
    public List<QuestObjectiveDto> Objectives { get; set; } = new();

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
}