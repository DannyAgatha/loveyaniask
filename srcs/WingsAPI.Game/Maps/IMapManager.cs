using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Maps;

public class StaticMapManager
{
    public static IMapManager Instance { get; private set; }

    public static void Initialize(IMapManager manager)
    {
        Instance = manager;
    }
}

public interface IMapManager
{
    Task Initialize();

    bool HasMapFlagByMapId(int mapId, MapFlags mapFlag);
    IReadOnlyList<MapFlags> GetMapFlagByMapId(int mapId);

    MapDataDTO GetMapByMapId(int mapId);

    /// <summary>
    ///     Returns the MapInstance with the corresponding ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IMapInstance GetMapInstance(Guid id);

    /// <summary>
    ///     Generates a new MapInstance
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IMapInstance GenerateMapInstanceByMapId(int mapId, MapInstanceType type);

    /// <summary>
    ///     Generates a new MapInstance
    /// </summary>
    /// <param name="serverMapDto"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IMapInstance GenerateMapInstanceByMapVNum(ServerMapDto serverMapDto, MapInstanceType type, List<MapFlags> flags = null);

    void RemoveMapInstance(Guid mapId);

    /// <summary>
    ///     Returns the mapinstance associated to the mapid
    ///     passed as parameter
    /// </summary>
    /// <param name="mapId"></param>
    /// <returns></returns>
    IMapInstance GetBaseMapInstanceByMapId(int mapId);

    Guid GetBaseMapInstanceIdByMapId(int mapId);

    /// <summary>
    ///     Teleports on a random place on the specified MapInstance
    /// </summary>
    /// <param name="session"></param>
    /// <param name="mapInstance"></param>
    /// <param name="isSameMap"></param>
    /// <returns></returns>
    Task TeleportOnRandomPlaceInMapAsync(IClientSession session, IMapInstance mapInstance, bool isSameMap = false);

    IEnumerable<IMapInstance> GetMapsWithFlag(MapFlags flags);

    /// <summary>
    ///     Returns the map with the specified name
    /// </summary>
    /// <param name="name">i18n key</param>
    /// <returns></returns>
    List<MapDataDTO> GetMapByName(string name);
    
    IMapInstance GeneratePrestigeMapInstance(int mapId);
    
    IMapInstance GeneratePrivateMapInstance(int mapId);
}