using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.Act4.Event;
using Plugin.Act4.Extension;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Enum;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.Event.Common;
using WingsAPI.Scripting.Object.Common;
using WingsAPI.Scripting.Object.Common.Map;
using WingsAPI.Scripting.Object.Dungeon;
using WingsAPI.Scripting.Object.Raid;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Entities;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Portals;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4;

public class DungeonFactory : IDungeonFactory
{
    private static readonly IEnumerable<Type> Converters;
    private readonly IAsyncEventPipeline _eventPipeline;

    private readonly HatusDragonHead _hatusHead = new()
    {
        BluePositionX = 23,
        RedPositionX = 36,
        GreenPositionX = 49
    };

    private readonly IMapManager _mapManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly IPortalFactory _portalFactory;
    private readonly IDungeonScriptManager _raidScriptManager;

    static DungeonFactory()
    {
        Converters = typeof(IScriptedEventConverter).Assembly.GetTypes()
            .Concat(typeof(DungeonFactory).Assembly.GetTypes())
            .Where(x => typeof(IScriptedEventConverter).IsAssignableFrom(x))
            .Where(x => !x.IsAbstract && !x.IsInterface);
    }

    public DungeonFactory(IDungeonScriptManager raidScriptManager, IMapManager mapManager, IAsyncEventPipeline eventPipeline, IMonsterEntityFactory monsterEntityFactory, IPortalFactory portalFactory)
    {
        _raidScriptManager = raidScriptManager;
        _mapManager = mapManager;
        _eventPipeline = eventPipeline;
        _monsterEntityFactory = monsterEntityFactory;
        _portalFactory = portalFactory;
    }

    public DungeonInstance CreateDungeon(long familyId, DungeonType dungeonType)
    {
        SDungeon scriptedDungeon = _raidScriptManager.GetScriptedDungeon(dungeonType.ToSDungeonType());
        if (scriptedDungeon == null)
        {
            Log.Error($"Unable to create dungeon of DungeonType: '{dungeonType.ToString()}', the script couldn't be found inside DungeonScriptManager",
                new Act4DungeonSystemException($"Unable to create act4 dungeon - DungeonType: {dungeonType.ToString()}, FamilyId: {familyId}"));
            return default;
        }

        var raidBoxRarities = new List<RaidBoxRarity>();

        foreach (SRaidBoxRarity boxRarity in scriptedDungeon.Reward.RaidBox.RaidBoxRarity)
        {
            var newRarity = new RaidBoxRarity(boxRarity.Rarity, boxRarity.Chance);
            raidBoxRarities.Add(newRarity);
        }

        var raidBox = new RaidBox(scriptedDungeon.Reward.RaidBox.RewardBox, raidBoxRarities);
        var raidReward = new RaidReward(raidBox, scriptedDungeon.Reward.DefaultReputation, scriptedDungeon.Reward.FixedReputation);

        //Preparing game object locators for the conversion of ScriptEvents to GameEvents
        Dictionary<Guid, DungeonSubInstance> dungeonSubInstancesByScriptId = new();
        Dictionary<Guid, IPortalEntity> portalsByScriptId = new();
        Dictionary<Guid, IMonsterEntity> monstersByScriptId = new();
        Dictionary<Guid, ButtonMapItem> buttonsByScriptId = new();

        Dictionary<Guid, List<(IEventTriggerContainer container, IDictionary<string, IEnumerable<SEvent>> events)>> eventsToConvertAndAdd = new();

        HatusHeads hatusHeads = dungeonType == DungeonType.Hatus ? new HatusHeads(_hatusHead) : null;

        //We prepare first portals to allow the creation of portals
        //(portals connect two maps and pre-categorizing the maps by their ScriptIds lets us obtain their GameIds, allowing us to generate gamePortals as we require of GameIds to do so)
        foreach (SMap scriptedMap in scriptedDungeon.Maps)
        {
            IMapInstance map = scriptedMap.MapType == SMapType.MapId
                ? _mapManager.GenerateMapInstanceByMapId(scriptedMap.MapIdVnum, MapInstanceType.Act4Dungeon)
                : _mapManager.GenerateMapInstanceByMapVNum(new ServerMapDto
                {
                    Flags = scriptedMap.Flags.Select(mapFlag => (MapFlags)(int)mapFlag).ToList(),
                    Id = scriptedMap.MapIdVnum,
                    MapVnum = scriptedMap.MapIdVnum,
                    NameId = scriptedMap.NameId,
                    MusicId = scriptedMap.MusicId
                }, MapInstanceType.Act4Dungeon);

            dungeonSubInstancesByScriptId[scriptedMap.Id] = new DungeonSubInstance(map, _eventPipeline, hatusHeads);
            eventsToConvertAndAdd[scriptedMap.Id] = new List<(IEventTriggerContainer container, IDictionary<string, IEnumerable<SEvent>> events)>
            {
                (dungeonSubInstancesByScriptId[scriptedMap.Id], scriptedMap.Events)
            };
        }

        foreach (SMap scriptedMap in scriptedDungeon.Maps)
        {
            DungeonSubInstance dungeonSubInstance = dungeonSubInstancesByScriptId[scriptedMap.Id];
            var dungeonWaves = new List<DungeonLinearWave>();
            var dungeonLoopWaves = new List<DungeonLoopWave>();

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
                        SpawnCell = mobWave.IsRandomPosition ? null : new Position(mobWave.Position.X, mobWave.Position.Y),
                        SummonType = SummonType.MONSTER_WAVE
                    });
                }

                if (monsterWave.Loop && monsterWave.LoopTick.HasValue)
                {
                    dungeonLoopWaves.Add(new DungeonLoopWave
                    {
                        Delay = TimeSpan.FromSeconds(monsterWave.TimeInSeconds),
                        Monsters = summons,
                        TickDelay = TimeSpan.FromSeconds(monsterWave.LoopTick.Value),
                        IsScaledWithPlayerAmount = monsterWave.IsScaledWithPlayerAmount
                    });
                }
                else
                {
                    dungeonWaves.Add(new DungeonLinearWave
                    {
                        Monsters = summons,
                        Delay = TimeSpan.FromSeconds(monsterWave.TimeInSeconds)
                    });
                }
            }
            
            foreach (SNpcWave npcWave in scriptedMap.NpcWaves)
            {
                var summons = new List<ToSummon>();

                foreach (SMapNpc npc in npcWave.Npcs)
                {
                    summons.Add(new ToSummon
                    {
                        VNum = npc.Vnum,
                        IsHostile = false, // Set based on your logic if NPCs are hostile or not
                        IsMoving = npc.CanMove,
                        SpawnCell = npc.IsProtectedNpc ? null : new Position(npc.Position.X, npc.Position.Y),
                        SummonType = SummonType.NPC_WAVE
                    });
                }

                if (npcWave.Loop && npcWave.LoopTick.HasValue)
                {
                    dungeonLoopWaves.Add(new DungeonLoopWave
                    {
                        Delay = TimeSpan.FromSeconds(npcWave.TimeInSeconds),
                        Monsters = summons,
                        TickDelay = TimeSpan.FromSeconds(npcWave.LoopTick.Value),
                        IsScaledWithPlayerAmount = npcWave.IsScaledWithPlayerAmount
                    });
                }
                else
                {
                    dungeonWaves.Add(new DungeonLinearWave
                    {
                        Monsters = summons,
                        Delay = TimeSpan.FromSeconds(npcWave.TimeInSeconds)
                    });
                }
            }
            dungeonSubInstance.SetLinearWaves(dungeonWaves);
            dungeonSubInstance.SetLoopWaves(dungeonLoopWaves);

            var portalGenerators = new List<PortalGenerator>();

            foreach (SPortal scriptedPortal in scriptedMap.Portals)
            {
                bool isReturnPortal = scriptedPortal.IsReturn;

                var sourcePos = new Position(scriptedPortal.SourcePosition.X, scriptedPortal.SourcePosition.Y);
                IMapInstance sourceMap = dungeonSubInstancesByScriptId[scriptedMap.Id].MapInstance;
                var destPos = new Position(scriptedPortal.DestinationPosition.X, scriptedPortal.DestinationPosition.Y);
                IMapInstance destinationMap = isReturnPortal ? null : dungeonSubInstancesByScriptId[scriptedPortal.DestinationId].MapInstance;
                IPortalEntity portal = _portalFactory.CreatePortal((PortalType)scriptedPortal.Type, sourceMap, sourcePos, destinationMap, destPos);

                portalsByScriptId[scriptedPortal.Id] = portal;

                if (scriptedPortal.CreationDelay.HasValue)
                {
                    portalGenerators.Add(new PortalGenerator
                    {
                        Portal = portal,
                        Delay = TimeSpan.FromSeconds(scriptedPortal.CreationDelay.Value)
                    });

                    continue;
                }

                dungeonSubInstance.MapInstance.AddPortalToMap(portal);
            }

            dungeonSubInstance.SetPortalGenerators(portalGenerators);

            // Add monsters
            foreach (SMonster scriptedMonster in scriptedMap.Monsters)
            {
                IMonsterEntity monster = _monsterEntityFactory.CreateMonster(scriptedMonster.Vnum, dungeonSubInstance.MapInstance,
                    new MonsterEntityBuilder
                    {
                        IsRespawningOnDeath = false,
                        IsBoss = scriptedMonster.IsBoss,
                        IsTarget = scriptedMonster.IsTarget,
                        IsWalkingAround = scriptedMonster.CanMove,
                        IsHostile = true
                    });

                monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, scriptedMonster.Position.X, scriptedMonster.Position.Y));

                monstersByScriptId[scriptedMonster.Id] = monster;
                eventsToConvertAndAdd[scriptedMap.Id].Add((monster, scriptedMonster.Events));
                //Adding this to obtain the subInstance via the monsters' Script Id
                dungeonSubInstancesByScriptId[scriptedMonster.Id] = dungeonSubInstance;

                dungeonSubInstance.AddDungeonMonster(monster);
            }
            
            // Add map objects
            foreach (SMapObject scriptedObject in scriptedMap.Objects)
            {
                if (scriptedObject is not SButton scriptedButton)
                {
                    continue;
                }

                var button = new ButtonMapItem(scriptedButton.Position.X, scriptedButton.Position.Y, scriptedButton.DeactivatedVnum, scriptedButton.ActivatedVnum, false,
                    dungeonSubInstance.MapInstance, _eventPipeline, scriptedButton.IsObjective);

                eventsToConvertAndAdd[scriptedMap.Id].Add((button, scriptedButton.Events));

                dungeonSubInstance.AddDungeonButton(button);
            }
        }

        //Yeah, we need an ugly wrapper because the instance is not constructed until the end, and our events require that instance for the future.
        DungeonInstanceWrapper dungeonInstanceWrapper = new();

        IServiceCollection serviceLocator = new ServiceCollection();

        serviceLocator.AddSingleton(dungeonSubInstancesByScriptId);
        serviceLocator.AddSingleton(portalsByScriptId);
        serviceLocator.AddSingleton(monstersByScriptId);
        serviceLocator.AddSingleton(buttonsByScriptId);
        serviceLocator.AddSingleton(dungeonInstanceWrapper);

        foreach (Type converter in Converters)
        {
            serviceLocator.AddTransient(typeof(IScriptedEventConverter), converter);
        }

        foreach ((Guid scriptedMapId, List<(IEventTriggerContainer container, IDictionary<string, IEnumerable<SEvent>> events)> eventsForContainers) in eventsToConvertAndAdd)
        {
            serviceLocator.AddSingleton(dungeonSubInstancesByScriptId[scriptedMapId]);

            var converters = serviceLocator.BuildServiceProvider()
                .GetServices<IScriptedEventConverter>()
                .ToDictionary(x => x.EventType, x => x);

            foreach ((IEventTriggerContainer container, IDictionary<string, IEnumerable<SEvent>> containerEvents) in eventsForContainers)
            {
                foreach ((string key, IEnumerable<SEvent> events) in containerEvents)
                {
                    foreach (SEvent scriptedEvent in events)
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

                        //quick win
                        if (attribute.Name == SRemovePortalEvent.Name)
                        {
                            container.AddEvent(key, new Act4DungeonBroadcastBossClosedEvent
                            {
                                DungeonInstanceWrapper = dungeonInstanceWrapper
                            }, attribute.IsRemovedOnTrigger);
                        }

                        container.AddEvent(key, e, attribute.IsRemovedOnTrigger);
                    }
                }
            }
        }

        return dungeonInstanceWrapper.DungeonInstance = new DungeonInstance(familyId, dungeonType, dungeonSubInstancesByScriptId.Values, dungeonSubInstancesByScriptId[scriptedDungeon.Spawn.MapId],
            new Position(scriptedDungeon.Spawn.Position.X, scriptedDungeon.Spawn.Position.Y), raidReward);
    }
}