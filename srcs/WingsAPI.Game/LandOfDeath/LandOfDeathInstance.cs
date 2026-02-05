using System;
using WingsAPI.Packets.Enums.LandOfDeath;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.LandOfDeath;

public class LandOfDeathInstance
{
    public LandOfDeathInstance(IMapInstance mapInstance)
    {
        MapInstance = mapInstance;
    }

    public DateTime CreationTime { get; set; }

    public IMapInstance MapInstance { get; }

    public IMonsterEntity DevilMonster { get; set; }

    // Get Player's ID for Devil's position to spawn (on player's death/join to map)
    public int? LastPlayerId { get; set; }
    
    public LandOfDeathMode Mode { get; set; }
}