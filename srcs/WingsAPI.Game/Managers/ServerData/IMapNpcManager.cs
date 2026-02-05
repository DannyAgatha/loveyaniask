using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;

namespace WingsEmu.Game.Managers.ServerData;

public interface IMapNpcManager
{
    /// <summary>
    ///     Gets the MapNpc from its id
    /// </summary>
    /// <returns></returns>
    MapNpcDTO GetById(int mapNpcId);

    /// <summary>
    ///     Returns all the map npcs that are supposedly contained in this mapId
    ///     All the time you call this method, you'r going to get new map npcs
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<MapNpcDTO> GetByMapId(int mapId);

    IReadOnlyList<MapNpcDTO> GetMapNpcsPerVNum(int vnum);

    /// <summary>
    ///     Loads all MapNpc's to the cache
    /// </summary>
    Task InitializeAsync();
}