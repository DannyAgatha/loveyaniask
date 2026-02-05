// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Quests;

public class QuestPrizeDto : IIntDto
{
    public int QuestId { get; set; }
    public byte RewardType { get; set; }
    public int Data0 { get; set; }
    public int Data1 { get; set; }
    public int Data2 { get; set; }
    public int Data3 { get; set; }
    public int Data4 { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}