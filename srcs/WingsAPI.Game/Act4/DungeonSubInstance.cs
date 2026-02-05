using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4.Entities;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Triggers;

namespace WingsEmu.Game.Act4;

public class DungeonSubInstance : IEventTriggerContainer
{
    private readonly List<IMonsterEntity> _bosses = new();

    private readonly IEventTriggerContainer _eventTriggerContainer;

    public DungeonSubInstance(IMapInstance mapInstance, IAsyncEventPipeline asyncEventPipeline, HatusHeads hatusHeads = null)
    {
        MapInstance = mapInstance;
        _eventTriggerContainer = new EventTriggerContainer(asyncEventPipeline);
        HatusHeads = hatusHeads;
    }

    public IMapInstance MapInstance { get; }

    public HatusHeads HatusHeads { get; }
    public IReadOnlyList<IMonsterEntity> Bosses => _bosses;

    public List<DungeonLoopWave> LoopWaves { get; private set; }
    public DateTime? LastDungeonWaveLoop { get; set; }

    public List<DungeonLinearWave> LinearWaves { get; private set; }
    public DateTime? LastDungeonWaveLinear { get; set; }

    public List<PortalGenerator> PortalGenerators { get; private set; }
    public DateTime? LastPortalGeneration { get; set; }

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false) => _eventTriggerContainer.AddEvent(key, notification, removedOnTrigger);
    public Task TriggerEvents(string key) => _eventTriggerContainer.TriggerEvents(key);

    public void SetLoopWaves(List<DungeonLoopWave> dungeonWaves)
    {
        LoopWaves = dungeonWaves;
    }

    public void SetLinearWaves(IEnumerable<DungeonLinearWave> dungeonWaves)
    {
        LinearWaves = dungeonWaves.OrderBy(x => x.Delay).ToList();
    }

    public void SetPortalGenerators(IEnumerable<PortalGenerator> portalGenerators)
    {
        PortalGenerators = portalGenerators.OrderBy(x => x.Delay).ToList();
    }

    public void AddDungeonMonster(IMonsterEntity monster)
    {
        if (monster.IsBoss)
        {
            _bosses.Add(monster);
        }
    }

    public void AddDungeonButton(ButtonMapItem buttonMapItem)
    {
        MapInstance.AddDrop(buttonMapItem);
    }
}

public class DungeonLoopWave
{
    public DateTime FirstSpawnWave { get; set; }
    public TimeSpan Delay { get; init; }
    public IReadOnlyList<ToSummon> Monsters { get; init; }
    public TimeSpan TickDelay { get; init; }
    public DateTime LastMonsterSpawn { get; set; }
    public bool IsScaledWithPlayerAmount { get; set; }
}

public class DungeonLinearWave
{
    public IReadOnlyList<ToSummon> Monsters { get; init; }
    public TimeSpan Delay { get; init; }
}

public class PortalGenerator
{
    public IPortalEntity Portal { get; init; }
    public TimeSpan Delay { get; init; }
}