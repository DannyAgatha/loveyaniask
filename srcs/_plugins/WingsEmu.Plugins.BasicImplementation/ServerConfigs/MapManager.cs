using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Maps;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Portals;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.GameData;
using WingsEmu.Core.Extensions;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class MapManager : IMapManager
{
    private readonly Dictionary<int, IMapInstance> _baseMapInstancesByMapId = new();
    private readonly SerializableGameServer _gameServer;
    private readonly Dictionary<int, ServerMapDto> _mapConfigByMapId = new();
    private readonly IEnumerable<ConfiguredMapImportFile> _mapConfigurations;

    private readonly Dictionary<int, MapDataDTO> _mapDataByMapVNum = new();
    private readonly Dictionary<int, HashSet<MapFlags>> _mapFlags = new();
    private readonly IMapInstanceFactory _mapInstanceFactory;
    private readonly ConcurrentDictionary<MapFlags, ThreadSafeList<IMapInstance>> _mapInstancesByFlags = new();
    private readonly ConcurrentDictionary<Guid, IMapInstance> _mapInstancesByGuid = new();
    private readonly IResourceLoader<MapDataDTO> _mapLoader;
    private readonly IMapMonsterManager _mapMonsterManager;
    private readonly IMapNpcManager _mapNpcManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcEntityFactory _npcEntityFactory;
    private readonly IEnumerable<PortalImportFile> _portalConfigurationFiles;
    private readonly Dictionary<int, List<PortalDTO>> _portalDataByMapId = new();
    private readonly ITimeSpaceConfiguration _timeSpaceConfiguration;
    private readonly ITimeSpacePortalFactory _timeSpacePortalFactory;
    private readonly IKeyValueCache<List<MapDataDTO>> _mapByName;

    public MapManager(IEnumerable<PortalImportFile> portalConfigurationFiles, IResourceLoader<MapDataDTO> mapLoader, IEnumerable<ConfiguredMapImportFile> mapConfigurations,
        SerializableGameServer gameServer, IMapInstanceFactory mapInstanceFactory,
        IMonsterEntityFactory monsterEntityFactory, INpcEntityFactory npcEntityFactory, IMapMonsterManager mapMonsterManager, IMapNpcManager mapNpcManager,
        ITimeSpaceConfiguration timeSpaceConfiguration, ITimeSpacePortalFactory timeSpacePortalFactory, IKeyValueCache<List<MapDataDTO>> mapByName)
    {
        _portalConfigurationFiles = portalConfigurationFiles;
        _mapLoader = mapLoader;
        _mapConfigurations = mapConfigurations;
        _gameServer = gameServer;
        _mapInstanceFactory = mapInstanceFactory;
        _monsterEntityFactory = monsterEntityFactory;
        _npcEntityFactory = npcEntityFactory;
        _mapMonsterManager = mapMonsterManager;
        _mapNpcManager = mapNpcManager;
        _timeSpaceConfiguration = timeSpaceConfiguration;
        _timeSpacePortalFactory = timeSpacePortalFactory;
        _mapByName = mapByName;
    }

    public async Task Initialize()
    {
        int count = 0;
        IEnumerable<MapDataDTO> maps = await _mapLoader.LoadAsync();
        foreach (MapDataDTO map in maps)
        {
            _mapDataByMapVNum[map.Id] = map;
            _mapByName.GetOrSet(map.Name, () => new List<MapDataDTO>()).Add(map);
            count++;
        }

        Log.Info($"[MAP_MANAGER] Loaded {count.ToString()} MapClientData");

        count = 0;
        IEnumerable<PortalDTO> portals = _portalConfigurationFiles.SelectMany(s => s.Portals.Select(p => p.ToDto())).ToList();
        foreach (PortalDTO portal in portals)
        {
            if (!_portalDataByMapId.TryGetValue(portal.SourceMapId, out List<PortalDTO> portalDtos))
            {
                portalDtos = _portalDataByMapId[portal.SourceMapId] = new List<PortalDTO>();
            }

            portalDtos.Add(portal);
            count++;
        }

        Log.Info($"[MAP_MANAGER] Loaded {count.ToString()} MapPortals");
        DateTime initTime = DateTime.UtcNow;
        count = 0;
        int countBaseMaps = 0;
        var configuredMaps = _mapConfigurations.SelectMany(s => s.Select(p => p.ToDto())).ToList();
        foreach (ServerMapDto configuredMap in configuredMaps)
        {
            // Load MapFlags at the beginning
            _mapFlags[configuredMap.Id] = configuredMap.Flags.ToHashSet();

            if (_gameServer.ChannelType == GameChannelType.ACT_4
                    ? !configuredMap.Flags.Contains(MapFlags.ACT_4) && !configuredMap.Flags.Contains(MapFlags.IS_MINILAND_MAP)
                    : configuredMap.Flags.Contains(MapFlags.ACT_4))
            {
                continue;
            }

            _mapConfigByMapId[configuredMap.Id] = configuredMap;
            count++;

            if (!configuredMap.Flags.Contains(MapFlags.IS_BASE_MAP))
            {
                continue;
            }

            IMapInstance mapInstance = GenerateMapInstanceByMapId(configuredMap.Id, MapInstanceType.BaseMapInstance);
            mapInstance.Initialize(initTime);
            _baseMapInstancesByMapId[configuredMap.Id] = mapInstance;
            countBaseMaps++;
        }

        Log.Info($"[MAP_MANAGER] Loaded {count.ToString()} MapConfigurations");
        Log.Info($"[MAP_MANAGER] Instantiated {countBaseMaps.ToString()} BaseMaps");
    }

    public MapDataDTO GetMapByMapId(int mapId) => _mapDataByMapVNum.GetOrDefault(mapId);

    public IMapInstance GenerateMapInstanceByMapId(int mapId, MapInstanceType type)
    {
        if (!_mapConfigByMapId.TryGetValue(mapId, out ServerMapDto mapConfiguration))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a ServerMapDto/MapConfiguration while trying to generate a MapInstance with MapId: '{mapId.ToString()}'");
            return null;
        }

        if (!_mapDataByMapVNum.TryGetValue(mapConfiguration.MapVnum, out MapDataDTO map))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a MapDataDto while trying to generate a MapInstance with MapVNum: '{mapConfiguration.MapVnum.ToString()}'");
            return null;
        }

        IMapInstance mapInstance = _mapInstanceFactory.CreateMap(new Map
        {
            Flags = mapConfiguration.Flags,
            Grid = map.Grid,
            Width = map.Width,
            Height = map.Height,
            MapId = mapConfiguration.Id,
            MapVnum = map.Id,
            MapNameId = mapConfiguration.NameId,
            Music = mapConfiguration.MusicId
        }, type);

        if (_portalDataByMapId.TryGetValue(mapId, out List<PortalDTO> portals))
        {
            mapInstance.LoadPortals(portals);
        }

        IEnumerable<TimeSpaceFileConfiguration> timeSpacePortals = _timeSpaceConfiguration.GetTimeSpaceConfigurationsByMapId(mapId);
        if (timeSpacePortals != null)
        {
            foreach (TimeSpaceFileConfiguration timeSpacePortal in timeSpacePortals)
            {
                foreach (TimeSpacePlacement placement in timeSpacePortal.Placement.Where(x => x.MapId == mapId))
                {
                    ITimeSpacePortalEntity portal = _timeSpacePortalFactory.CreateTimeSpacePortal(timeSpacePortal, new Position(placement.MapX, placement.MapY));
                    mapInstance.TimeSpacePortals.Add(portal);
                }
            }
        }

        IEnumerable<MapMonsterDTO> monsters = _mapMonsterManager.GetByMapId(mapId);
        if (monsters != null)
        {
            foreach (MapMonsterDTO monster in monsters)
            {
                try
                {
                    IMonsterEntity mapMonster = _monsterEntityFactory.CreateMapMonster(monster, mapInstance);
                    mapMonster.EmitEvent(new MapJoinMonsterEntityEvent(mapMonster));
                }
                catch
                {
                    Log.Warn("[MOB] Couldn't load monster: " + monster.MonsterVNum + $" on map: {mapId}");
                }
            }
        }

        IEnumerable<MapNpcDTO> npcs = _mapNpcManager.GetByMapId(mapId);
        if (npcs != null)
        {
            foreach (MapNpcDTO npc in npcs)
            {
                try
                {
                    INpcEntity npcEntity = _npcEntityFactory.CreateMapNpc(npc, mapInstance);
                    npcEntity.EmitEvent(new MapJoinNpcEntityEvent(npcEntity));
                }
                catch
                {
                    Log.Warn("[NPC] Couldn't load NPC: " + npc.NpcVNum + $" on map: {mapId}");
                }
            }
        }

        _mapInstancesByGuid.TryAdd(mapInstance.Id, mapInstance);
        foreach (MapFlags flag in mapConfiguration.Flags)
        {
            _mapInstancesByFlags.GetOrAdd(flag, new ThreadSafeList<IMapInstance>()).Add(mapInstance);
        }

        return mapInstance;
    }

    public IMapInstance GenerateMapInstanceByMapVNum(ServerMapDto serverMapDto, MapInstanceType type, List<MapFlags> flags = null)
    {
        if (!_mapDataByMapVNum.TryGetValue(serverMapDto.MapVnum, out MapDataDTO map))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a MapDataDto while trying to generate a MapInstance with MapVNum: '{serverMapDto.MapVnum.ToString()}'");
            return null;
        }
        
        flags ??= serverMapDto.Flags;

        IMapInstance mapInstance = _mapInstanceFactory.CreateMap(new Map
        {
            Flags = flags,
            Grid = map.Grid,
            Width = map.Width,
            Height = map.Height,
            MapId = serverMapDto.Id,
            MapVnum = map.Id,
            MapNameId = serverMapDto.NameId,
            Music = serverMapDto.MusicId,
            IsBossRoom = serverMapDto.IsBossRoom
        }, type);

        _mapInstancesByGuid.TryAdd(mapInstance.Id, mapInstance);
        
        foreach (MapFlags flag in flags)
        {
            _mapInstancesByFlags.GetOrAdd(flag, new ThreadSafeList<IMapInstance>()).Add(mapInstance);
        }

        return mapInstance;
    }

    public async Task TeleportOnRandomPlaceInMapAsync(IClientSession session, IMapInstance mapInstance, bool isSameMap = false)
    {
        if (mapInstance == default)
        {
            return;
        }

        Position pos = mapInstance.GetRandomPosition();

        switch (isSameMap)
        {
            case false:
                session.ChangeMap(mapInstance, pos.X, pos.Y);
                break;
            case true:
                session.PlayerEntity.TeleportOnMap(pos.X, pos.Y);
                break;
        }
    }

    public IEnumerable<IMapInstance> GetMapsWithFlag(MapFlags flags)
    {
        if (!_mapInstancesByFlags.TryGetValue(flags, out ThreadSafeList<IMapInstance> maps) || maps == null)
        {
            return Array.Empty<IMapInstance>();
        }

        return maps;
    }

    public IMapInstance GetMapInstance(Guid id) => _mapInstancesByGuid.GetOrDefault(id);

    public IMapInstance GetBaseMapInstanceByMapId(int mapId) => _baseMapInstancesByMapId.GetOrDefault(mapId);

    public Guid GetBaseMapInstanceIdByMapId(int mapId) => _baseMapInstancesByMapId.TryGetValue(mapId, out IMapInstance map) ? map.Id : Guid.Empty;

    public List<MapDataDTO> GetMapByName(string name) => _mapByName.Get(name);
    
    public IMapInstance GeneratePrestigeMapInstance(int mapId)
    {
        if (!_mapConfigByMapId.TryGetValue(mapId, out ServerMapDto mapConfiguration))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a ServerMapDto/MapConfiguration while trying to generate a MapInstance with MapId: '{mapId.ToString()}'");
            return null;
        }

        if (!_mapDataByMapVNum.TryGetValue(mapConfiguration.MapVnum, out MapDataDTO map))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a MapDataDto while trying to generate a MapInstance with MapVNum: '{mapConfiguration.MapVnum.ToString()}'");
            return null;
        }

        IMapInstance mapInstance = _mapInstanceFactory.CreateMap(new Map
        {
            Flags = mapConfiguration.Flags.Where(x => x != MapFlags.IS_BASE_MAP).ToList(),
            Grid = map.Grid,
            Width = map.Width,
            Height = map.Height,
            MapId = mapConfiguration.Id,
            MapVnum = map.Id,
            MapNameId = mapConfiguration.NameId,
            Music = mapConfiguration.MusicId
        }, MapInstanceType.PrestigeInstance);

        IEnumerable<MapMonsterDTO> monsters = _mapMonsterManager.GetByMapId(mapId);
        if (monsters == null || !monsters.Any())
        {
            return null;
        }

        foreach (MapMonsterDTO monster in monsters)
        {
            try
            {
                IMonsterEntity mapMonster = _monsterEntityFactory.CreateMapMonster(monster, mapInstance);
                mapMonster.EmitEvent(new MapJoinMonsterEntityEvent(mapMonster));
            }
            catch
            {
                Log.Warn("[MOB] Couldn't load monster: " + monster.MonsterVNum + $" on map: {mapId}");
            }
        }

        _mapInstancesByGuid.TryAdd(mapInstance.Id, mapInstance);
        
        foreach (MapFlags flag in mapConfiguration.Flags.Where(flag => flag != MapFlags.IS_BASE_MAP))
        {
            _mapInstancesByFlags.GetOrAdd(flag, new ThreadSafeList<IMapInstance>()).Add(mapInstance);
        }

        return mapInstance;
    }
    
    public IMapInstance GeneratePrivateMapInstance(int mapId)
    {
        if (!_mapConfigByMapId.TryGetValue(mapId, out ServerMapDto mapConfiguration))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a ServerMapDto/MapConfiguration while trying to generate a MapInstance with MapId: '{mapId.ToString()}'");
            return null;
        }

        if (!_mapDataByMapVNum.TryGetValue(mapConfiguration.MapVnum, out MapDataDTO map))
        {
            Log.Warn($"[MAP_MANAGER] Couldn't find a MapDataDto while trying to generate a MapInstance with MapVNum: '{mapConfiguration.MapVnum.ToString()}'");
            return null;
        }

        IMapInstance mapInstance = _mapInstanceFactory.CreateMap(new Map
        {
            Flags = mapConfiguration.Flags.Where(x => x != MapFlags.IS_BASE_MAP).ToList(),
            Grid = map.Grid,
            Width = map.Width,
            Height = map.Height,
            MapId = mapConfiguration.Id,
            MapVnum = map.Id,
            MapNameId = mapConfiguration.NameId,
            Music = mapConfiguration.MusicId
        }, MapInstanceType.PrivateInstance);

        IEnumerable<MapMonsterDTO> monsters = _mapMonsterManager.GetByMapId(mapId);
        if (monsters == null || !monsters.Any())
        {
            return null;
        }

        foreach (MapMonsterDTO monster in monsters)
        {
            try
            {
                IMonsterEntity mapMonster = _monsterEntityFactory.CreateMapMonster(monster, mapInstance);
                mapMonster.EmitEvent(new MapJoinMonsterEntityEvent(mapMonster));
            }
            catch
            {
                Log.Warn("[MOB] Couldn't load monster: " + monster.MonsterVNum + $" on map: {mapId}");
            }
        }

        _mapInstancesByGuid.TryAdd(mapInstance.Id, mapInstance);
        
        foreach (MapFlags flag in mapConfiguration.Flags.Where(flag => flag != MapFlags.IS_BASE_MAP))
        {
            _mapInstancesByFlags.GetOrAdd(flag, new ThreadSafeList<IMapInstance>()).Add(mapInstance);
        }

        return mapInstance;
    }

    public bool HasMapFlagByMapId(int mapId, MapFlags mapFlag)
    {
        if (!_mapFlags.TryGetValue(mapId, out HashSet<MapFlags> mapFlags))
        {
            return false;
        }

        return mapFlags != null && mapFlags.Contains(mapFlag);
    }

    public IReadOnlyList<MapFlags> GetMapFlagByMapId(int mapId) => _mapFlags.TryGetValue(mapId, out HashSet<MapFlags> mapFlags) ? mapFlags.ToList() : Array.Empty<MapFlags>();

    public void RemoveMapInstance(Guid mapId)
    {
        if (!_mapInstancesByGuid.TryRemove(mapId, out IMapInstance map))
        {
            return;
        }

        if (map.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            _baseMapInstancesByMapId.Remove(map.MapId);
        }

        if (_mapConfigByMapId.TryGetValue(map.MapId, out ServerMapDto mapConfiguration))
        {
            foreach (MapFlags flag in mapConfiguration.Flags)
            {
                if (_mapInstancesByFlags.TryGetValue(flag, out ThreadSafeList<IMapInstance> list) && list != null)
                {
                    list.Remove(map);
                }
            }
        }

        map.Destroy();
    }
}