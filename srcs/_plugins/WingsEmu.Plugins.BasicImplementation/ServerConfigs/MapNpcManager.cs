using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Managers.ServerData;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class MapNpcManager : IMapNpcManager
{
    private readonly ILongKeyCachedRepository<MapNpcDTO> _mapNpcById;
    private readonly IEnumerable<MapNpcImportFile> _mapNpcConfigurations;
    private readonly IKeyValueCache<List<MapNpcDTO>> _mapNpcs;

    public MapNpcManager(IEnumerable<MapNpcImportFile> mapNpcConfigurations, ILongKeyCachedRepository<MapNpcDTO> mapNpcById, IKeyValueCache<List<MapNpcDTO>> mapNpcs)
    {
        _mapNpcConfigurations = mapNpcConfigurations;
        _mapNpcById = mapNpcById;
        _mapNpcs = mapNpcs;
    }

    public async Task InitializeAsync()
    {
        IEnumerable<MapNpcObject> importedNpcs = _mapNpcConfigurations.SelectMany(x => x.Npcs.Select(s =>
        {
            s.MapId = x.MapId;
            return s;
        }));

        var npcs = importedNpcs.Select(s => s.ToDto()).ToList();

        int count = 0;
        foreach (MapNpcDTO npcDto in npcs)
        {
            _mapNpcById.Set(npcDto.Id, npcDto);
            _mapNpcs.GetOrSet($"by-map-id-{npcDto.MapId.ToString()}", () => new List<MapNpcDTO>()).Add(npcDto);
            _mapNpcs.GetOrSet($"by-npc-vnum-{npcDto.NpcVNum.ToString()}", () => new List<MapNpcDTO>()).Add(npcDto);
            count++;
        }

        Log.Info($"[DATABASE] Loaded {count.ToString()} MapNPCs");
    }

    public MapNpcDTO GetById(int mapNpcId) => _mapNpcById.Get(mapNpcId);

    public IReadOnlyList<MapNpcDTO> GetByMapId(int mapId) => _mapNpcs.Get($"by-map-id-{mapId.ToString()}");

    public IReadOnlyList<MapNpcDTO> GetMapNpcsPerVNum(int npcMonsterVnum) => _mapNpcs.Get($"by-npc-vnum-{npcMonsterVnum.ToString()}");
}