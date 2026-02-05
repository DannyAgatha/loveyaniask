using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Quests;

public class QuestObjectiveDto : IIntDto
{
    public int QuestId { get; set; }
    public int Data0 { get; set; }
    public int Data1 { get; set; }
    public int Data2 { get; set; }
    public int Data3 { get; set; }
    public byte ObjectiveIndex { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}