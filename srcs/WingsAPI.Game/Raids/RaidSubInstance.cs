using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Triggers;

namespace WingsEmu.Game.Raids;

public class RaidSubInstance : IEventTriggerContainer
{
    private readonly List<IMonsterEntity> _bossMonsters = new();
    
    private readonly List<INpcEntity> _npcEntities = new();
    
    private readonly List<IMonsterEntity> _deadBossMonsters = new();

    private readonly IEventTriggerContainer _eventTriggerContainer;

    public RaidSubInstance(IMapInstance mapInstance, IAsyncEventPipeline asyncEventPipeline)
    {
        MapInstance = mapInstance;
        _eventTriggerContainer = new EventTriggerContainer(asyncEventPipeline);
    }

    public IDictionary<byte, RaidWave> RaidWaves { get; private set; }

    public bool RaidWavesActivated { get; set; }
    public byte RaidWaveState { get; set; }
    public DateTime LastRaidWave { get; set; } = DateTime.MaxValue;

    public bool TargetsCompleted => CurrentCompletedTargetMonsters >= CurrentTargetMonsters && CurrentCompletedTargetButtons >= CurrentTargetButtons;

    public IMapInstance MapInstance { get; }

    public IReadOnlyCollection<IMonsterEntity> BossMonsters
    {
        get
        {
            CleanMonsters();
            return _bossMonsters;
        }
    }

    public IReadOnlyCollection<IMonsterEntity> DeadBossMonsters
    {
        get
        {
            CleanMonsters();
            return _deadBossMonsters;
        }
    }

    public int CurrentCompletedTargetMonsters { get; set; }
    public int CurrentTargetMonsters { get; private set; }

    public int CurrentCompletedTargetButtons { get; set; }

    public int CurrentTargetButtons { get; private set; }
    public bool IsDiscoveredByLeader { get; set; }
    
    public Position? SavedTargetPosition { get; set; }
    public DateTime? LordDracoMeteorsSpawn { get; set; }
    
    public IReadOnlyCollection<(int, Position)> GlacerusSafePositions { get; set; }
    
    public DateTime LastGlacerusFreezeTime { get; set; }

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false) => _eventTriggerContainer.AddEvent(key, notification, removedOnTrigger);

    public Task TriggerEvents(string key) => _eventTriggerContainer.TriggerEvents(key);

    public void AddRaidMonster(IMonsterEntity monsterEntity)
    {
        if (monsterEntity.IsTarget || monsterEntity.IsBoss)
        {
            CurrentTargetMonsters++;
        }

        if (monsterEntity.IsBoss)
        {
            _bossMonsters.Add(monsterEntity);
        }
    }

    public void AddRaidNpc(INpcEntity npcEntity)
    {
        if (npcEntity.IsProtected)
        {
            _npcEntities.Add(npcEntity);
        }
    }

    public void RemoveRaidMonster(IMonsterEntity monsterEntity)
    {
        if (monsterEntity.IsTarget)
        {
            CurrentTargetMonsters--;
        }

        if (monsterEntity.IsBoss)
        {
            _bossMonsters.Remove(monsterEntity);
        }

        MapInstance.RemoveMonster(monsterEntity);
    }

    public void AddRaidButton(ButtonMapItem buttonMapItem)
    {
        MapInstance.AddDrop(buttonMapItem);
        if (!buttonMapItem.IsObjective)
        {
            return;
        }

        CurrentTargetButtons++;
    }

    public void RemoveRaidButton(ButtonMapItem buttonMapItem)
    {
        MapInstance.RemoveDrop(buttonMapItem.TransportId);
        if (!buttonMapItem.IsObjective)
        {
            return;
        }

        CurrentTargetButtons--;
    }

    public void AddRaidWave(Dictionary<byte, RaidWave> raidWaves)
    {
        RaidWaves = raidWaves;
        RaidWaveState = 0;
    }

    private void CleanMonsters()
    {
        foreach (IMonsterEntity monsterEntity in _bossMonsters.ToArray())
        {
            if (monsterEntity.IsAlive())
            {
                continue;
            }

            _bossMonsters.Remove(monsterEntity);
            _deadBossMonsters.Add(monsterEntity);
        }
    }
}