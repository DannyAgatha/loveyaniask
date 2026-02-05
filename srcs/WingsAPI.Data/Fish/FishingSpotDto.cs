using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WingsAPI.Data.Fish;

public class FishingSpotDto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long FishVnum { get; set; }

    public int MaxLvl { get; set; }

    public int MinLvl { get; set; }

    public long MapId { get; set; }

    public List<FishingRewardsDto> Rewards { get; set; } = new();
    public List<FishingPathDto> Paths { get; set; } = new();
}