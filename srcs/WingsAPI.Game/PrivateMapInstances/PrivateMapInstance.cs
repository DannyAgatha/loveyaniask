using System;
using WingsEmu.Game.Maps;
using WingsEmu.Game.PrivateMapInstances.Events;

namespace WingsEmu.Game.PrivateMapInstances;

public class PrivateMapInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public IMapInstance MapInstance { get; init; }
    
    public PrivateMapInstanceType Type { get; init; }
    
    public DateTime StartTime { get; init; }
    
    public DateTime EndTime { get; init; }
    
    public int? PlayerId { get; init; }
    
    public int? GroupId { get; init; }
    
    public long? FamilyId { get; init; }
    
    public int? MapVnum { get; init; }
    
    public bool IsPremium { get; set; }
    
    public bool IsHardcore { get; set; }
}

public class PrivateMapInstanceInfo
{
    public DateTime Start { get; } = DateTime.UtcNow;
    
    public PrivateMapInstanceType Type { get; set; }
    
    public long ItemsCollected { get; set; }
    
    public long GoldCollected { get; set; }
    
    public long FlowerBuffsCollected { get; set; }
    
    public long MonstersKilled { get; set; }
    
    public ulong ExperienceGained { get; set; }
    
    public ulong HeroExperienceGained { get; set; }
    
    public ulong JobExperienceGained { get; set; }
    
    public byte LevelGained { get; set; }
    
    public byte HeroLevelGained { get; set; }
}