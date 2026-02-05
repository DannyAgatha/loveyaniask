using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Monsters;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Managers.ServerData;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class MapMonsterManager : IMapMonsterManager
{
    private readonly IEnumerable<MapMonsterImportFile> _files;
    private readonly ILongKeyCachedRepository<MapMonsterDTO> _mapMonsterById;
    private readonly IKeyValueCache<List<MapMonsterDTO>> _mapMonsters;

    public MapMonsterManager(IEnumerable<MapMonsterImportFile> files, ILongKeyCachedRepository<MapMonsterDTO> mapMonsterById, IKeyValueCache<List<MapMonsterDTO>> mapMonsters)
    {
        _files = files;
        _mapMonsterById = mapMonsterById;
        _mapMonsters = mapMonsters;
    }

    public async Task InitializeAsync()
    {
        var monsters = _files.SelectMany(x => x.Monsters.Select(s =>
        {
            s.MapId = x.MapId;
            return s.ToDto();
        })).ToList();

        int count = 0;
        foreach (MapMonsterDTO npcDto in monsters)
        {
            _mapMonsterById.Set(npcDto.Id, npcDto);
            _mapMonsters.GetOrSet($"by-map-id-{npcDto.MapId.ToString()}", () => new List<MapMonsterDTO>()).Add(npcDto);
            _mapMonsters.GetOrSet($"by-monster-vnum-{npcDto.MonsterVNum.ToString()}", () => new List<MapMonsterDTO>()).Add(npcDto);
            count++;
        }

        Log.Info($"[DATABASE] Loaded {count.ToString()} MapMonsters");
    }

    public MapMonsterDTO GetById(int mapNpcId) => _mapMonsterById.Get(mapNpcId);

    public IReadOnlyList<MapMonsterDTO> GetByMapId(int mapId) => _mapMonsters.Get($"by-map-id-{mapId.ToString()}");

    public IReadOnlyList<MapMonsterDTO> GetMapMonstersPerVNum(int npcMonsterVnum) => _mapMonsters.Get($"by-monster-vnum-{npcMonsterVnum.ToString()}");
}