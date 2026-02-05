using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Enum;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.Object.Common;
using WingsAPI.Scripting.Object.Common.Map;
using WingsAPI.Scripting.Object.Timespace;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Portals;
using WingsEmu.Game.Raids;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Packets.Enums;

namespace Plugin.TimeSpaces;

public class TimeSpaceFactory : ITimeSpaceFactory
{
    private static readonly IEnumerable<Type> Converters;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IMapManager _mapManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcEntityFactory _npcEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IPortalFactory _portalFactory;
    private readonly ITimeSpaceScriptManager _timeSpaceScriptManager;

    static TimeSpaceFactory()
    {
        Converters = typeof(IScriptedEventConverter).Assembly.GetTypes()
            .Concat(typeof(TimeSpaceFactory).Assembly.GetTypes())
            .Where(x => typeof(IScriptedEventConverter).IsAssignableFrom(x))
            .Where(x => !x.IsAbstract && !x.IsInterface);
    }

    public TimeSpaceFactory(ITimeSpaceScriptManager timeSpaceScriptManager, IMapManager mapManager, IAsyncEventPipeline eventPipeline, IMonsterEntityFactory monsterEntityFactory,
        INpcMonsterManager npcMonsterManager, IPortalFactory portalFactory, INpcEntityFactory npcEntityFactory, IGameItemInstanceFactory gameItemInstance)
    {
        _timeSpaceScriptManager = timeSpaceScriptManager;
        _mapManager = mapManager;
        _eventPipeline = eventPipeline;
        _monsterEntityFactory = monsterEntityFactory;
        _npcMonsterManager = npcMonsterManager;
        _portalFactory = portalFactory;
        _npcEntityFactory = npcEntityFactory;
        _gameItemInstance = gameItemInstance;
    }

    public TimeSpaceInstance Create(TimeSpaceParty timeSpaceParty)
    {
        try
        {
            ScriptTimeSpace scriptedTimeSpace = _timeSpaceScriptManager.GetScriptedTimeSpace(timeSpaceParty.TimeSpaceId);
            if (scriptedTimeSpace == null)
            {
                Log.Warn($"Can't create TimeSpace {timeSpaceParty.TimeSpaceId}, Couldn't find it in {nameof(LuaTimeSpaceScriptManager)}");
                return null;
            }

            IServiceCollection serviceLocator = new ServiceCollection();

            foreach (Type converter in Converters)
            {
                serviceLocator.AddTransient(typeof(IScriptedEventConverter), converter);
            }

            var timeSpaceSubInstances = new Dictionary<Guid, TimeSpaceSubInstance>();
            var tasks = new Dictionary<Guid, TimeSpaceTask>();
            var protectedNpcs = new HashSet<long>();

            int mapId = 20000;
            foreach (SMap scriptedMap in scriptedTimeSpace.Maps)
            {
                IMapInstance map;
                if (scriptedMap.MapType == SMapType.MapId)
                {
                    map = _mapManager.GenerateMapInstanceByMapId(scriptedMap.MapIdVnum, MapInstanceType.TimeSpaceInstance);
                }
                else
                {
                    map = _mapManager.GenerateMapInstanceByMapVNum(new ServerMapDto
                    {
                        Flags = scriptedMap.Flags.Select(mapFlag => (MapFlags)(int)mapFlag).Where(x => x != MapFlags.IS_BASE_MAP).ToList(),
                        Id = mapId,
                        MapVnum = scriptedMap.MapIdVnum,
                        NameId = scriptedMap.NameId,
                        MusicId = scriptedMap.MusicId
                    }, MapInstanceType.TimeSpaceInstance);

                    mapId++;
                }

                map.MapIndexX = scriptedMap.MapIndexX;
                map.MapIndexY = scriptedMap.MapIndexY;
                map.AIDisabled = true;
                timeSpaceSubInstances[scriptedMap.Id] = new TimeSpaceSubInstance(map, _eventPipeline);
            }

            var objective = new TimeSpaceObjective
            {
                KillAllMonsters = scriptedTimeSpace.Objectives.KillAllMonsters,
                GoToExit = scriptedTimeSpace.Objectives.GoToExit,
                ProtectNPC = scriptedTimeSpace.Objectives.ProtectNPC,
                KillMonsterVnum = scriptedTimeSpace.Objectives.KillMonsterVnum,
                KillMonsterAmount = scriptedTimeSpace.Objectives.KillMonsterAmount,
                CollectItemVnum = scriptedTimeSpace.Objectives.CollectItemVnum,
                CollectItemAmount = scriptedTimeSpace.Objectives.CollectItemAmount,
                Conversation = scriptedTimeSpace.Objectives.Conversation,
                InteractObjectsVnum = scriptedTimeSpace.Objectives.InteractObjectsVnum,
                InteractObjectsAmount = scriptedTimeSpace.Objectives.InteractObjectsAmount
            };

            serviceLocator.AddSingleton(timeSpaceSubInstances);
            serviceLocator.AddSingleton(tasks);

            var portals = new Dictionary<Guid, IPortalEntity>();
            var monsters = new Dictionary<Guid, IMonsterEntity>();
            var npcs = new Dictionary<Guid, INpcEntity>();
            var buttons = new Dictionary<Guid, ButtonMapItem>();
            var items = new Dictionary<Guid, TimeSpaceMapItem>();
            var eventConverters = new Dictionary<Type, IScriptedEventConverter>();

            // Register portals, monsters and buttons to service locator
            serviceLocator.AddSingleton(portals);
            serviceLocator.AddSingleton(monsters);
            serviceLocator.AddSingleton(buttons);
            serviceLocator.AddSingleton(npcs);
            serviceLocator.AddSingleton(items);
            serviceLocator.AddSingleton(timeSpaceParty);
            serviceLocator.AddSingleton(eventConverters);

            // Load first portals and monster waves for later event converter usages
            foreach (SMap map in scriptedTimeSpace.Maps)
            {
                TimeSpaceSubInstance timeSpaceSubInstance = timeSpaceSubInstances[map.Id];
                IMapInstance mapInstance = timeSpaceSubInstance.MapInstance;

                AddPortals(map, timeSpaceSubInstances, portals, mapInstance);
                AddMonsterWaves(map, timeSpaceSubInstance);
            }

            int npcId = 100000;
            int monsterId = 100000;
            foreach (SMap scriptedMap in scriptedTimeSpace.Maps)
            {
                TimeSpaceSubInstance timeSpaceSubInstance = timeSpaceSubInstances[scriptedMap.Id];
                IMapInstance mapInstance = timeSpaceSubInstance.MapInstance;

                serviceLocator.AddSingleton(timeSpaceSubInstance);
                serviceLocator.AddSingleton(mapInstance);

                // This dictionary will contains all events to attach later
                var events = new Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>>
                {
                    [timeSpaceSubInstance] = scriptedMap.Events
                };

                if (scriptedMap.TimeSpaceTask != null)
                {
                    var task = new TimeSpaceTask
                    {
                        TaskType = (TimeSpaceTaskType)scriptedMap.TimeSpaceTask.TimeSpaceTaskType,
                        Time = scriptedMap.TimeSpaceTask.DurationInSeconds == null ? null : TimeSpan.FromSeconds(scriptedMap.TimeSpaceTask.DurationInSeconds.Value),
                        GameDialogKey = scriptedMap.TimeSpaceTask.GameDialogKey,
                        StartDialog = scriptedMap.TimeSpaceTask.StartDialog,
                        EndDialog = scriptedMap.TimeSpaceTask.EndDialog,
                        StartDialogShout = scriptedMap.TimeSpaceTask.StartDialogShout,
                        EndDialogShout = scriptedMap.TimeSpaceTask.EndDialogShout,
                        DialogStartTask = scriptedMap.TimeSpaceTask.DialogStartTask,
                        StartDialogIsObjective = scriptedMap.TimeSpaceTask.StartDialogIsObjective,
                        EndDialogIsObjective = scriptedMap.TimeSpaceTask.EndDialogIsObjective
                    };

                    timeSpaceSubInstance.Task = task;
                    tasks[scriptedMap.Id] = task;
                }

                AddMonsters(scriptedMap, mapInstance, monsters, events, timeSpaceSubInstance, ref monsterId);
                AddNpcs(scriptedMap, mapInstance, npcs, events, protectedNpcs, ref npcId);
                AddMapObjects(scriptedMap, mapInstance, events, items, buttons);

                AddEventConverters(serviceLocator, events, eventConverters);
            }

            return new TimeSpaceInstance(timeSpaceSubInstances.Values, timeSpaceSubInstances[scriptedTimeSpace.Spawn.MapId],
                new Position(scriptedTimeSpace.Spawn.Position.X, scriptedTimeSpace.Spawn.Position.Y),
                TimeSpan.FromSeconds(timeSpaceParty.IsEasyMode ? scriptedTimeSpace.DurationInSeconds * 2 : scriptedTimeSpace.DurationInSeconds),
                scriptedTimeSpace.Lives, objective, scriptedTimeSpace.BonusPointItemDropChance, protectedNpcs,
                scriptedTimeSpace.ObtainablePartnerVnum,
                scriptedTimeSpace.InfiniteDuration, scriptedTimeSpace.PreFinishDialog, scriptedTimeSpace.PreFinishDialogIsObjective);
        }
        catch (Exception e)
        {
            Log.Error("TimeSpaceFactory", e);
            return null;
        }
    }

    private void AddMonsterWaves(SMap scriptedMap, TimeSpaceSubInstance timeSpaceSubInstance)
    {
        var timeSpaceWaves = new List<TimeSpaceWave>();

        // Add Monster Waves
        foreach (SMonsterWave monsterWave in scriptedMap.MonsterWaves)
        {
            var summons = new List<ToSummon>();

            foreach (SMonster mobWave in monsterWave.Monsters)
            {
                summons.Add(new ToSummon
                {
                    VNum = mobWave.Vnum,
                    IsHostile = true,
                    IsMoving = true,
                    HpMultiplier = mobWave.HpMultiplier,
                    MpMultiplier = mobWave.MpMultiplier,
                    SpawnCell = mobWave.IsRandomPosition ? null : new Position(mobWave.Position.X, mobWave.Position.Y),
                    Direction = mobWave.Direction,
                    Level = mobWave.CustomLevel,
                    SummonType = SummonType.MONSTER_WAVE
                });
            }

            timeSpaceWaves.Add(new TimeSpaceWave
            {
                Monsters = summons,
                Delay = TimeSpan.FromSeconds(monsterWave.TimeInSeconds)
            });
        }

        timeSpaceSubInstance.TimeSpaceWaves = timeSpaceWaves;
    }

    private void AddNpcs(SMap scriptedMap, IMapInstance mapInstance, Dictionary<Guid, INpcEntity> npcs, Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events,
        HashSet<long> protectedNpcs, ref int npcId)
    {
        foreach (SMapNpc scriptedNpc in scriptedMap.Npcs)
        {
            npcId++;
            INpcEntity npc = _npcEntityFactory.CreateNpc(scriptedNpc.Vnum, mapInstance, npcId, new NpcAdditionalData
            {
                IsProtected = scriptedNpc.IsProtectedNpc,
                IsTimeSpaceMate = scriptedNpc.FollowPlayer,
                CanMove = scriptedNpc.CanMove,
                NpcDirection = scriptedNpc.Direction,
                CanAttack = true,
                IsHostile = true,
                HpMultiplier = scriptedNpc.HpMultiplier,
                MpMultiplier = scriptedNpc.MpMultiplier,
                CustomLevel = scriptedNpc.CustomLevel
            });

            if (scriptedNpc.IsProtectedNpc)
            {
                protectedNpcs.Add(npc.Id);
            }

            npc.EmitEventAsync(new MapJoinNpcEntityEvent(npc, scriptedNpc.Position.X, scriptedNpc.Position.Y));

            npcs[scriptedNpc.Id] = npc;
            events[npc] = scriptedNpc.Events;
        }
    }

    private static void AddEventConverters(IServiceCollection serviceLocator, Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events,
        Dictionary<Type, IScriptedEventConverter> eventConverters)
    {
        // Get event converters using service locator
        var converters = serviceLocator.BuildServiceProvider()
            .GetServices<IScriptedEventConverter>()
            .ToDictionary(x => x.EventType, x => x);

        foreach ((Type key, IScriptedEventConverter value) in converters)
        {
            eventConverters[key] = value;
        }

        // Add all events using event converters
        foreach ((IEventTriggerContainer eventContainer, IDictionary<string, IEnumerable<SEvent>> containerEvents) in events)
        {
            foreach ((string trigger, IEnumerable<SEvent> scriptedEvents) in containerEvents)
            {
                foreach (SEvent scriptedEvent in scriptedEvents)
                {
                    IAsyncEvent e = converters.GetValueOrDefault(scriptedEvent.GetType())?.Convert(scriptedEvent);
                    if (e == null)
                    {
                        throw new InvalidOperationException($"Failed to convert {scriptedEvent.GetType().Name} to async event");
                    }

                    ScriptEventAttribute attribute = scriptedEvent.GetType().GetCustomAttribute<ScriptEventAttribute>();
                    if (attribute == null)
                    {
                        throw new InvalidOperationException($"Failed to find attribute for: {scriptedEvent.GetType().Name}");
                    }

                    eventContainer.AddEvent(trigger, e, attribute.IsRemovedOnTrigger);
                }
            }
        }
    }

    private void AddMapObjects(SMap scriptedMap, IMapInstance mapInstance, Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events,
        Dictionary<Guid, TimeSpaceMapItem> items, Dictionary<Guid, ButtonMapItem> buttons)
    {
        // Add map objects
        foreach (SMapObject scriptedObject in scriptedMap.Objects)
        {
            switch (scriptedObject)
            {
                case SButton scriptedButton:

                    short positionX = scriptedButton.Position.X;
                    short positionY = scriptedButton.Position.Y;

                    if (scriptedButton.IsRandomPosition)
                    {
                        Position getPosition = mapInstance.GetRandomPosition();
                        positionX = getPosition.X;
                        positionY = getPosition.Y;
                    }

                    var button = new ButtonMapItem(positionX, positionY, scriptedButton.DeactivatedVnum, scriptedButton.ActivatedVnum,
                        false, mapInstance, _eventPipeline, scriptedButton.OnlyOnce == false ? null : false, scriptedButton.IsObjective, false,
                        scriptedButton.CustomDanceDuration);

                    events[button] = scriptedButton.Events;
                    buttons[scriptedButton.Id] = button;
                    mapInstance.AddDrop(button);
                    break;
                case SItem scriptedItem:
                    GameItemInstance newItem = _gameItemInstance.CreateItem(scriptedItem.Vnum);
                    positionX = scriptedItem.Position.X;
                    positionY = scriptedItem.Position.Y;

                    if (scriptedItem.IsRandomPosition)
                    {
                        Position getPosition = mapInstance.GetRandomPosition();
                        positionX = getPosition.X;
                        positionY = getPosition.Y;
                    }

                    if (scriptedItem.IsRandomUniquePosition)
                    {
                        Position getPosition;
                        while (true)
                        {
                            getPosition = mapInstance.GetRandomPosition();
                            MapItem findOtherItemInPosition = mapInstance.Drops.FirstOrDefault(x => x.PositionX == getPosition.X && x.PositionY == getPosition.Y);
                            if (findOtherItemInPosition == null)
                            {
                                break;
                            }
                        }

                        positionX = getPosition.X;
                        positionY = getPosition.Y;
                    }

                    var item = new TimeSpaceMapItem(positionX, positionY, false, newItem,
                        _eventPipeline, mapInstance, scriptedItem.DanceDuration, scriptedItem.IsObjective);

                    events[item] = scriptedItem.Events;
                    items[scriptedItem.Id] = item;
                    mapInstance.AddDrop(item);
                    break;
            }
        }
    }

    private void AddMonsters(SMap scriptedMap, IMapInstance mapInstance, IDictionary<Guid, IMonsterEntity> monsters,
        IDictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events, TimeSpaceSubInstance timeSpaceSubInstance, ref int monsterId)
    {
        // Add monsters
        foreach (SMonster scriptedMonster in scriptedMap.Monsters)
        {
            monsterId++;
            short positionX = scriptedMonster.Position.X;
            short positionY = scriptedMonster.Position.Y;

            if (scriptedMonster.SpawnAfterTask && timeSpaceSubInstance.Task != null)
            {
                if (scriptedMonster.IsRandomPosition)
                {
                    Position position = mapInstance.GetRandomPosition();
                    positionX = position.X;
                    positionY = position.Y;
                }

                IMonsterEntity monsterAfterTask = _monsterEntityFactory.CreateMonster(monsterId, scriptedMonster.Vnum, mapInstance,
                    new MonsterEntityBuilder
                    {
                        IsRespawningOnDeath = false,
                        IsBoss = scriptedMonster.IsBoss,
                        IsTarget = scriptedMonster.IsTarget,
                        IsWalkingAround = scriptedMonster.CanMove,
                        IsHostile = true,
                        HpMultiplier = scriptedMonster.HpMultiplier,
                        MpMultiplier = scriptedMonster.MpMultiplier,
                        PositionX = positionX,
                        PositionY = positionY,
                        Direction = scriptedMonster.Direction,
                        Level = scriptedMonster.CustomLevel
                    });

                monsters[scriptedMonster.Id] = monsterAfterTask;
                events[monsterAfterTask] = scriptedMonster.Events;
                timeSpaceSubInstance.Task.MonstersAfterTaskStart.Add((scriptedMonster.SpawnAfterMobs == 0 ? null : scriptedMonster.SpawnAfterMobs, monsterAfterTask));
                continue;
            }

            if (scriptedMonster.SpawnAfterMobs != 0)
            {
                if (scriptedMonster.IsRandomPosition)
                {
                    Position position = mapInstance.GetRandomPosition();
                    positionX = position.X;
                    positionY = position.Y;
                }

                IMonsterEntity spawnAfterMobs = _monsterEntityFactory.CreateMonster(monsterId, scriptedMonster.Vnum, mapInstance,
                    new MonsterEntityBuilder
                    {
                        IsRespawningOnDeath = false,
                        IsBoss = scriptedMonster.IsBoss,
                        IsTarget = scriptedMonster.IsTarget,
                        IsWalkingAround = scriptedMonster.CanMove,
                        IsHostile = true,
                        HpMultiplier = scriptedMonster.HpMultiplier,
                        MpMultiplier = scriptedMonster.MpMultiplier,
                        PositionX = positionX,
                        PositionY = positionY,
                        Direction = scriptedMonster.Direction,
                        Level = scriptedMonster.CustomLevel
                    });

                monsters[scriptedMonster.Id] = spawnAfterMobs;
                events[spawnAfterMobs] = scriptedMonster.Events;

                if (!timeSpaceSubInstance.SpawnAfterMobsKilled.TryGetValue(scriptedMonster.SpawnAfterMobs, out List<IMonsterEntity> list))
                {
                    list = new List<IMonsterEntity>();
                    timeSpaceSubInstance.SpawnAfterMobsKilled[scriptedMonster.SpawnAfterMobs] = list;
                }

                list.Add(spawnAfterMobs);
                continue;
            }

            if (scriptedMonster.IsRandomPosition)
            {
                Position position = mapInstance.GetRandomPosition();
                positionX = position.X;
                positionY = position.Y;
            }

            IMonsterEntity monster = _monsterEntityFactory.CreateMonster(monsterId, scriptedMonster.Vnum, mapInstance,
                new MonsterEntityBuilder
                {
                    IsRespawningOnDeath = false,
                    IsBoss = scriptedMonster.IsBoss,
                    IsTarget = scriptedMonster.IsTarget,
                    IsWalkingAround = scriptedMonster.CanMove,
                    IsHostile = true,
                    HpMultiplier = scriptedMonster.HpMultiplier,
                    MpMultiplier = scriptedMonster.MpMultiplier,
                    Direction = scriptedMonster.Direction,
                    Level = scriptedMonster.CustomLevel
                });

            monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, positionX, positionY));

            monsters[scriptedMonster.Id] = monster;
            events[monster] = scriptedMonster.Events;
        }
    }

    private void AddPortals(SMap scriptedMap, IReadOnlyDictionary<Guid, TimeSpaceSubInstance> raidSubInstances, Dictionary<Guid, IPortalEntity> portals, IMapInstance mapInstance)
    {
        // Add portals
        foreach (SPortal scriptedPortal in scriptedMap.Portals)
        {
            var sourcePosition = new Position(scriptedPortal.SourcePosition.X, scriptedPortal.SourcePosition.Y);
            var destinationPos = new Position(scriptedPortal.DestinationPosition.X, scriptedPortal.DestinationPosition.Y);
            var portalOrient = (PortalMinimapOrientation)scriptedPortal.PortalMiniMapOrientation;

            IPortalEntity portal = _portalFactory.CreatePortal((PortalType)scriptedPortal.Type, mapInstance, sourcePosition, raidSubInstances[scriptedPortal.DestinationId].MapInstance,
                destinationPos, portalOrient);

            portals[scriptedPortal.Id] = portal;
            mapInstance.Portals.Add(portal);
        }
    }
}