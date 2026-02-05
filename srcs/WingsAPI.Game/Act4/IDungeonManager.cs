using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WingsEmu.Game.Act4.Entities;

namespace WingsEmu.Game.Act4;

public interface IDungeonManager
{
    void AddNewHatusState(Guid mapInstanceId, HatusState hatusState);
    HatusState GetHatusState(Guid mapInstanceId);
    void RemoveHatusState(Guid mapInstanceId);

    void AddCalvinasDragons(Guid mapInstanceId, CalvinasState calvinasState);
    CalvinasState GetCalvinasDragons(Guid mapInstanceId);
    void RemoveCalvinasDragons(Guid mapInstanceId);
}

public class DungeonManager : IDungeonManager
{
    private readonly ConcurrentDictionary<Guid, CalvinasState> _calvinasStates = new();
    private readonly ConcurrentDictionary<Guid, HatusState> _hatusStates = new();

    public void AddNewHatusState(Guid mapInstanceId, HatusState hatusState)
    {
        _hatusStates.TryAdd(mapInstanceId, hatusState);
    }

    public HatusState GetHatusState(Guid mapInstanceId) => !_hatusStates.TryGetValue(mapInstanceId, out HatusState hatusState) ? null : hatusState;

    public void RemoveHatusState(Guid mapInstanceId)
    {
        _hatusStates.TryRemove(mapInstanceId, out _);
    }

    public void AddCalvinasDragons(Guid mapInstanceId, CalvinasState calvinasState)
    {
        _calvinasStates.TryAdd(mapInstanceId, calvinasState);
    }

    public CalvinasState GetCalvinasDragons(Guid mapInstanceId) => !_calvinasStates.TryGetValue(mapInstanceId, out CalvinasState calvinasState) ? null : calvinasState;

    public void RemoveCalvinasDragons(Guid mapInstanceId)
    {
        _calvinasStates.TryRemove(mapInstanceId, out _);
    }
}

public class HatusState
{
    public TimeSpan CastTime { get; init; }
    public double DealtDamage { get; init; }

    public bool BlueAttack { get; set; }
    public short BlueX { get; set; }

    public bool RedAttack { get; set; }
    public short RedX { get; set; }

    public bool GreenAttack { get; set; }
    public short GreenX { get; set; }
}

public class CalvinasState
{
    public DateTime CastTime { get; init; }

    public List<CalvinasDragon> CalvinasDragonsList { get; init; }
}