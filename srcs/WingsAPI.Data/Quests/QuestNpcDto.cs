using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Quests;

public class QuestNpcDto : IIntDto
{
    public int? QuestId { get; set; }
    public short NpcVnum { get; set; }
    public short Level { get; set; }
    public short StartingScript { get; set; }
    public short RequiredCompletedScript { get; set; }
    public bool IsMainQuest { get; set; }
    public short MapId { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}