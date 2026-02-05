using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.Configurations.Gold;

public class GeneralMapGoldConfiguration
{
    public List<MapGoldConfiguration> GeneralMapGold { get; set; } = [];
    
    public MapGoldConfiguration? GetGoldConfigurationByMapId(int mapId)
    {
        return GeneralMapGold.FirstOrDefault(config => config.MapVnums.Contains(mapId));
    }
}

public class MapGoldConfiguration
{
    public List<int> MapVnums { get; set; } = [];
    public int MinRange { get; set; }
    public int MaxRange { get; set; }
}