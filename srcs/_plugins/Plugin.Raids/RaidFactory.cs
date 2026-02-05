using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.Raids.Extension;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Enum;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.Object.Common;
using WingsAPI.Scripting.Object.Common.Map;
using WingsAPI.Scripting.Object.Raid;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Portals;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;

namespace Plugin.Raids;

public class RaidFactory : IRaidFactory
{
    private static readonly IEnumerable<Type> Converters;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IMapManager _mapManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcEntityFactory _npcEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IRaidScriptManager _raidScriptManager;
    private readonly IBossScalingConfiguration _bossScalingConfiguration;

    static RaidFactory()
    {
        Converters = typeof(IScriptedEventConverter).Assembly.GetTypes()
            .Concat(typeof(RaidFactory).Assembly.GetTypes())
            .Where(x => typeof(IScriptedEventConverter).IsAssignableFrom(x))
            .Where(x => !x.IsAbstract && !x.IsInterface);
    }

    public RaidFactory(IRaidScriptManager raidScriptManager, IMapManager mapManager, IAsyncEventPipeline eventPipeline, IMonsterEntityFactory monsterEntityFactory,
        INpcMonsterManager npcMonsterManager, IPortalFactory portalFactory, INpcEntityFactory npcEntityFactory, IBossScalingConfiguration bossScalingConfiguration)
    {
        _raidScriptManager = raidScriptManager;
        _mapManager = mapManager;
        _eventPipeline = eventPipeline;
        _monsterEntityFactory = monsterEntityFactory;
        _npcMonsterManager = npcMonsterManager;
        _portalFactory = portalFactory;
        _npcEntityFactory = npcEntityFactory;
        _bossScalingConfiguration = bossScalingConfiguration;
    }

    public RaidInstance CreateRaid(RaidParty raidParty)
    {
        SRaid scriptedRaid = _raidScriptManager.GetScriptedRaid(raidParty.Type.ToSRaidType());
        if (scriptedRaid == null)
        {
            Log.Warn($"Can't create raid {raidParty.Type}, Couldn't find it in RaidScriptManager");
            return default;
        }

        IServiceCollection serviceLocator = new ServiceCollection();

        foreach (Type converter in Converters)
        {
            serviceLocator.AddTransient(typeof(IScriptedEventConverter), converter);
        }

        var raidSubInstances = new Dictionary<Guid, RaidSubInstance>();
        GenerateMaps(scriptedRaid, raidSubInstances);

        RaidReward raidReward = InitializeRaidRewards(scriptedRaid);

        serviceLocator.AddSingleton(raidSubInstances);

        var portals = new Dictionary<Guid, IPortalEntity>();
        var monsters = new Dictionary<Guid, IMonsterEntity>();
        var npcs = new Dictionary<Guid, INpcEntity>();
        var buttons = new Dictionary<Guid, ButtonMapItem>();

        // Register portals, monsters and buttons to service locator
        serviceLocator.AddSingleton(portals);
        serviceLocator.AddSingleton(monsters);
        serviceLocator.AddSingleton(npcs);
        serviceLocator.AddSingleton(buttons);

        foreach (SMap scriptedMap in scriptedRaid.Maps)
        {
            RaidSubInstance raidSubInstance = raidSubInstances[scriptedMap.Id];
            IMapInstance mapInstance = raidSubInstance.MapInstance;

            serviceLocator.AddSingleton(raidParty);
            serviceLocator.AddSingleton(raidSubInstance);
            serviceLocator.AddSingleton(mapInstance);


            // This dictionary will contains all events to attach later
            var events = new Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>>
            {
                [raidSubInstance] = scriptedMap.Events
            };

            // Add Raid Waves
            AddWaves(scriptedMap, raidSubInstance);
            AddPortals(scriptedMap, mapInstance, raidSubInstances, portals);
            AddMonsters(scriptedMap, mapInstance, monsters, events, raidSubInstance);
            AddNpcs(scriptedMap, mapInstance, npcs, events, raidSubInstance);
            AddMapObjects(scriptedMap, mapInstance, events, raidSubInstance, buttons);

            // Get event converters using service locator
            var converters = serviceLocator.BuildServiceProvider()
                .GetServices<IScriptedEventConverter>()
                .ToDictionary(x => x.EventType, x => x);

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
        
        if (_bossScalingConfiguration != null)
        {
            IMonsterEntity monster = monsters.FirstOrDefault(s => s.Value.IsBoss).Value;
            int raidMembersCount = raidParty.Members.Count;
            int bossHpPercentage = _bossScalingConfiguration.GetBossHpPercentageByPlayersAndRaidType(raidParty.Type, raidMembersCount);

            monster.Hp = (int)(monster.MaxHp * (bossHpPercentage * 0.01));
            monster.FakeMaxHp = monster.Hp;
        }

        return new RaidInstance(raidSubInstances.Values, raidSubInstances[scriptedRaid.Spawn.MapId], new Position(scriptedRaid.Spawn.Position.X, scriptedRaid.Spawn.Position.Y),
            TimeSpan.FromSeconds(scriptedRaid.DurationInSeconds), (byte)raidParty.Members.Count, raidReward);
    }
    
    private void AddNpcs(SMap scriptedMap, IMapInstance mapInstance, Dictionary<Guid, INpcEntity> npcs,
        Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events, RaidSubInstance raidSubInstance)
    {
        foreach (SMapNpc scriptedNpc in scriptedMap.Npcs)
        {
            INpcEntity npc = _npcEntityFactory.CreateNpc(scriptedNpc.Vnum, mapInstance, null, new NpcAdditionalData
            {
                IsProtected = scriptedNpc.IsProtectedNpc,
                CanMove = scriptedNpc.CanMove,
                NpcDirection = scriptedNpc.Direction,
                CanAttack = true,
                IsHostile = true,
                HpMultiplier = scriptedNpc.HpMultiplier,
                MpMultiplier = scriptedNpc.MpMultiplier,
                CustomLevel = scriptedNpc.CustomLevel,
                NpcShouldRespawn = false
            });

            npc.EmitEventAsync(new MapJoinNpcEntityEvent(npc, scriptedNpc.Position.X, scriptedNpc.Position.Y));

            npcs[scriptedNpc.Id] = npc;
            events[npc] = scriptedNpc.Events;
            raidSubInstance.AddRaidNpc(npc);
        }
    }
    private void AddWaves(SMap scriptedMap, RaidSubInstance raidSubInstance)
    {
        var raidWaves = new Dictionary<byte, RaidWave>();
        byte wave = 0;
        foreach (SMonsterWave scriptedRaidWave in scriptedMap.MonsterWaves)
        {
            var waveMonsters = new List<ToSummon>();

            foreach (SMonster mobWave in scriptedRaidWave.Monsters)
            {
                IMonsterData monsterData = _npcMonsterManager.GetNpc(mobWave.Vnum);
                if (monsterData == null)
                {
                    continue;
                }

                ConcurrentDictionary<byte, Waypoint> waypointsDictionary = null;
                if (mobWave.Waypoints != null)
                {
                    byte waypoints = 0;
                    foreach (SWaypoint waypoint in mobWave.Waypoints)
                    {
                        waypointsDictionary ??= new ConcurrentDictionary<byte, Waypoint>();

                        var newWaypoint = new Waypoint
                        {
                            X = waypoint.X,
                            Y = waypoint.Y,
                            WaitTime = waypoint.WaitTime
                        };

                        waypointsDictionary.TryAdd(waypoints, newWaypoint);
                        waypoints++;
                    }
                }

                var newToSummon = new ToSummon
                {
                    VNum = mobWave.Vnum,
                    SpawnCell = mobWave.IsRandomPosition ? null : new Position(mobWave.Position.X, mobWave.Position.Y),
                    IsHostile = true,
                    IsMoving = mobWave.CanMove,
                    SetHitChance = 5,
                    GoToBossPosition = mobWave.GoToBossPosition == default ? null : new Position(mobWave.GoToBossPosition.X, mobWave.GoToBossPosition.Y),
                    Waypoints = waypointsDictionary,
                    SummonType = SummonType.MONSTER_WAVE,
                    Direction = mobWave.Direction,
                    IgnoreSkillRange = mobWave.IgnoreSkillRange
                };

                waveMonsters.Add(newToSummon);
            }

            var newRaidWave = new RaidWave(waveMonsters, scriptedRaidWave.TimeInSeconds);

            raidWaves[wave] = newRaidWave;
            wave++;
        }
        
        foreach (SNpcWave scriptedNpcWave in scriptedMap.NpcWaves)
        {
            var summons = new List<ToSummon>();

            foreach (SMapNpc npcWave in scriptedNpcWave.Npcs)
            {
                var summon = new ToSummon
                {
                    VNum = npcWave.Vnum,
                    SpawnCell = new Position(npcWave.Position.X, npcWave.Position.Y),
                    IsMoving = npcWave.CanMove,
                    IsHostile = false,
                    SummonType = SummonType.NPC_WAVE,
                    IsBonusOrProtected = npcWave.IsProtectedNpc
                };
                
                summons.Add(summon);

                _eventPipeline.ProcessEventAsync(new NpcSummonEvent
                {
                    Map = raidSubInstance.MapInstance,
                    MonsterId = npcWave.Id,
                    Npcs = summons
                });
                
                var newRaidWave = new RaidWave(summons, scriptedNpcWave.TimeInSeconds);

                raidWaves[wave] = newRaidWave;
                wave++;
            }

            if (raidSubInstance.MapInstance == null)
            {
                Console.WriteLine("raidSubInstance.MapInstance is not initialized.");
                return;
            }
        }
        raidSubInstance.AddRaidWave(raidWaves);
    }

    private void AddMapObjects(SMap scriptedMap, IMapInstance mapInstance, Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events, RaidSubInstance raidSubInstance,
        Dictionary<Guid, ButtonMapItem> buttons)
    {
        foreach (SMapObject scriptedObject in scriptedMap.Objects)
        {
            if (scriptedObject is not SButton scriptedButton)
            {
                continue;
            }

            var button = new ButtonMapItem(scriptedButton.Position.X, scriptedButton.Position.Y, scriptedButton.DeactivatedVnum, scriptedButton.ActivatedVnum, false,
                mapInstance, _eventPipeline, isObjective: scriptedButton.IsObjective, onlyOnce: scriptedButton.OnlyOnce == false ? null : false);

            events[button] = scriptedButton.Events;
            buttons[scriptedObject.Id] = button;
            raidSubInstance.AddRaidButton(button);
        }
    }

    private void AddMonsters(SMap scriptedMap, IMapInstance mapInstance, Dictionary<Guid, IMonsterEntity> monsters, Dictionary<IEventTriggerContainer, IDictionary<string, IEnumerable<SEvent>>> events,
        RaidSubInstance raidSubInstance)
    {
        // Add monsters
        foreach (SMonster scriptedMonster in scriptedMap.Monsters)
        {
            var drop = new List<DropChance>();
            foreach (SDropChance dropChance in scriptedMonster.Drop)
            {
                drop.Add(new DropChance(dropChance.Chance, dropChance.ItemVnum, dropChance.Amount));
            }

            IMonsterEntity monster = _monsterEntityFactory.CreateMonster(scriptedMonster.Vnum, mapInstance,
                new MonsterEntityBuilder
                {
                    IsRespawningOnDeath = false,
                    IsBoss = scriptedMonster.IsBoss,
                    IsTarget = scriptedMonster.IsTarget,
                    IsWalkingAround = scriptedMonster.CanMove,
                    IsHostile = true,
                    SetHitChance = 5,
                    HpMultiplier = scriptedMonster.IsBoss ? null : scriptedMonster.IsTarget ? 21f : 6f,
                    MpMultiplier = scriptedMonster.IsBoss ? null : scriptedMonster.IsTarget ? 21f : 6f,
                    RaidDrop = drop,
                    GeneratedGuid = scriptedMonster.Id,
                    Direction = scriptedMonster.Direction,
                    IgnoreSkillRange = scriptedMonster.IgnoreSkillRange
                });

            if (scriptedMonster.Waypoints != null)
            {
                byte waypoints = 0;
                foreach (SWaypoint waypoint in scriptedMonster.Waypoints)
                {
                    monster.Waypoints ??= new ConcurrentDictionary<byte, Waypoint>();

                    var newWaypoint = new Waypoint
                    {
                        X = waypoint.X,
                        Y = waypoint.Y,
                        WaitTime = waypoint.WaitTime
                    };

                    monster.Waypoints.TryAdd(waypoints, newWaypoint);
                    waypoints++;
                }
            }

            monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, scriptedMonster.Position.X, scriptedMonster.Position.Y));

            monsters[scriptedMonster.Id] = monster;
            events[monster] = scriptedMonster.Events;

            raidSubInstance.AddRaidMonster(monster);
        }
    }

    private void AddPortals(SMap scriptedMap, IMapInstance mapInstance, Dictionary<Guid, RaidSubInstance> raidSubInstances, Dictionary<Guid, IPortalEntity> portals)
    {
        // Add portals
        foreach (SPortal scriptedPortal in scriptedMap.Portals)
        {
            var sourcePos = new Position(scriptedPortal.SourcePosition.X, scriptedPortal.SourcePosition.Y);
            var destPos = new Position(scriptedPortal.DestinationPosition.X, scriptedPortal.DestinationPosition.Y);
            IPortalEntity portal = _portalFactory.CreatePortal((PortalType)scriptedPortal.Type, mapInstance, sourcePos,
                raidSubInstances[scriptedPortal.DestinationId].MapInstance.Id, destPos);

            portals[scriptedPortal.Id] = portal;

            mapInstance.AddPortalToMap(portal);
        }
    }

    private static RaidReward InitializeRaidRewards(SRaid scriptedRaid)
    {
        var raidBoxRarities = new List<RaidBoxRarity>();

        foreach (SRaidBoxRarity boxRarity in scriptedRaid.Reward.RaidBox.RaidBoxRarity)
        {
            var newRarity = new RaidBoxRarity(boxRarity.Rarity, boxRarity.Chance);
            raidBoxRarities.Add(newRarity);
        }

        var raidBox = new RaidBox(scriptedRaid.Reward.RaidBox.RewardBox, raidBoxRarities);
        var raidReward = new RaidReward(raidBox, scriptedRaid.Reward.DefaultReputation, scriptedRaid.Reward.FixedReputation);
        return raidReward;
    }

    private void GenerateMaps(SRaid scriptedRaid, Dictionary<Guid, RaidSubInstance> raidSubInstances)
    {
        foreach (SMap scriptedMap in scriptedRaid.Maps)
        {
            IMapInstance map = scriptedMap.MapType == SMapType.MapId
                ? _mapManager.GenerateMapInstanceByMapId(scriptedMap.MapIdVnum, MapInstanceType.RaidInstance)
                : _mapManager.GenerateMapInstanceByMapVNum(new ServerMapDto
                {
                    Flags = scriptedMap.Flags.Select(mapFlag => (MapFlags)(int)mapFlag).ToList(),
                    Id = scriptedMap.MapIdVnum,
                    MapVnum = scriptedMap.MapIdVnum,
                    NameId = scriptedMap.NameId,
                    MusicId = scriptedMap.MusicId,
                    IsBossRoom = scriptedMap.IsBossRoom
                }, MapInstanceType.RaidInstance);

            raidSubInstances[scriptedMap.Id] = new RaidSubInstance(map, _eventPipeline);
        }
    }
}