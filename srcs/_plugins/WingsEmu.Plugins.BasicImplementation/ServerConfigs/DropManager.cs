// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Drops;
using PhoenixLib.Caching;
using WingsAPI.Data.Drops;
using WingsEmu.Game._enum;
using WingsEmu.Game.Managers.ServerData;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class DropManager : IDropManager
{
    private static readonly List<DropDTO> EmptyList = new();
    private readonly IKeyValueCache<List<DropDTO>> _dropCache;
    private readonly IEnumerable<DropImportFile> _dropConfigurations;

    private readonly List<DropDTO> _generalDrops = new();

    public DropManager(IEnumerable<DropImportFile> dropConfigurations, IKeyValueCache<List<DropDTO>> dropCache)
    {
        _dropConfigurations = dropConfigurations;
        _dropCache = dropCache;
    }

    public async Task InitializeAsync()
    {
        var drops = new List<DropDTO>();

        foreach (DropImportFile dropImportExportFile in _dropConfigurations)
        {
            drops.AddRange(dropImportExportFile.Drops.SelectMany(s => s.ToDto()));
        }

        foreach (DropDTO drop in drops)
        {
            if (drop.MonsterVNum != null)
            {
                string key = $"monsterVnum-{drop.MonsterVNum.Value}";
                List<DropDTO> list = _dropCache.Get(key);
                if (list == null)
                {
                    list = new List<DropDTO> { drop };
                    _dropCache.Set(key, list);
                    continue;
                }

                list.Add(drop);
                continue;
            }

            if (drop.MapId != null)
            {
                string key = $"mapId-{drop.MapId.Value}";
                List<DropDTO> list = _dropCache.Get(key);
                if (list == null)
                {
                    list = new List<DropDTO> { drop };
                    _dropCache.Set(key, list);
                    continue;
                }

                list.Add(drop);
                continue;
            }

            if (drop.RaceType != null && drop.RaceSubType != null)
            {
                string key = $"race-{drop.RaceType.Value}.{drop.RaceSubType.Value}";
                List<DropDTO> list = _dropCache.Get(key);
                if (list == null)
                {
                    list = new List<DropDTO> { drop };
                    _dropCache.Set(key, list);
                    continue;
                }

                list.Add(drop);
                continue;
            }

            _generalDrops.Add(drop);
        }
    }

    public IEnumerable<DropDTO> GetGeneralDrops() => _generalDrops;

    public IReadOnlyList<DropDTO> GetDropsByMapId(int mapId) => _dropCache.Get($"mapId-{mapId}") ?? EmptyList;

    public IReadOnlyList<DropDTO> GetDropsByMonsterVnum(int monsterVnum) => _dropCache.Get($"monsterVnum-{monsterVnum}") ?? EmptyList;

    public IReadOnlyList<DropDTO> GetDropsByMonsterRace(MonsterRaceType monsterRaceType, byte monsterSubRaceType) => _dropCache.Get($"race-{(byte)monsterRaceType}.{monsterSubRaceType}") ?? EmptyList;
}