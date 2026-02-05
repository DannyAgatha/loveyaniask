using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;

namespace WingsEmu.Game.Managers.ServerData;

public interface IMapMonsterManager
{
    /// <summary>
    ///     Gets the MapNpc from its id
    /// </summary>
    /// <returns></returns>
    MapMonsterDTO GetById(int mapNpcId);

    /// <summary>
    ///     Returns all the map npcs that are supposedly contained in this mapId
    ///     All the time you call this method, you'r going to get new map npcs
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<MapMonsterDTO> GetByMapId(int mapId);

    IReadOnlyList<MapMonsterDTO> GetMapMonstersPerVNum(int vnum);

    /// <summary>
    ///     Loads all MapNpc's to the cache
    /// </summary>
    Task InitializeAsync();
}