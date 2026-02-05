using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Triggers;

namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceSubInstance : IEventTriggerContainer
{
    private readonly IEventTriggerContainer _eventTriggerContainer;

    public TimeSpaceSubInstance(IMapInstance mapInstance, IAsyncEventPipeline asyncEventPipeline)
    {
        MapInstance = mapInstance;
        _eventTriggerContainer = new EventTriggerContainer(asyncEventPipeline);
    }

    public IMapInstance MapInstance { get; }

    public DateTime? TimeSpaceWave { get; set; }
    public List<TimeSpaceWave> TimeSpaceWaves { get; set; } = new();

    public TimeSpaceTask Task { get; set; }
    public long? MonsterBonusId { get; set; }
    public int MonsterBonusCombo { get; set; }
    public bool SendPortalOpenMessage { get; set; }
    public DateTime LastTryFinishTime { get; set; }

    public Dictionary<int, List<IMonsterEntity>> SpawnAfterMobsKilled { get; } = new();

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false) => _eventTriggerContainer.AddEvent(key, notification, removedOnTrigger);

    public Task TriggerEvents(string key) => _eventTriggerContainer.TriggerEvents(key);
}

public class TimeSpaceWave
{
    public IReadOnlyList<ToSummon> Monsters { get; init; }
    public TimeSpan Delay { get; init; }
}