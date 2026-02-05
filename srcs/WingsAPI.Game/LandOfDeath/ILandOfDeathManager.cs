using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums.LandOfDeath;

namespace WingsEmu.Game.LandOfDeath;

public interface ILandOfDeathManager
{
    bool IsActive { get; set; }
    bool IsDevilActive { get; set; }

    DateTime Start { get; set; }
    DateTime End { get; set; }

    DateTime NextDevilSpawn { get; set; }
    DateTime? NextDevilDespawn { get; set; }

    IReadOnlyCollection<LandOfDeathInstance> Instances { get; }

    LandOfDeathInstance GetLandOfDeathInstanceByFamilyId(long familyId, LandOfDeathMode mode);
    LandOfDeathInstance GetLandOfDeathInstanceByPlayerId(int playerId);
    LandOfDeathInstance GetLandOfDeathInstanceByGroupId(int groupId, LandOfDeathMode mode);
    LandOfDeathInstance GetLandOfDeathInstanceByPublic(LandOfDeathMode mode);

    void AddLandOfDeathInstanceByFamily(long familyId, LandOfDeathMode mode, LandOfDeathInstance instance);
    void AddLandOfDeathInstanceByPlayer(int playerId, LandOfDeathInstance instance);
    void AddLandOfDeathInstanceByGroup(int groupId, LandOfDeathMode mode, LandOfDeathInstance instance);
    void AddLandOfDeathInstanceByPublic(LandOfDeathMode mode, LandOfDeathInstance instance);

    void RemoveLandOfDeathInstance(LandOfDeathInstance instance);
    IEnumerable<LandOfDeathInstance> GetGroupLandOfDeathInstances();
    void Clear();
}

public class LandOfDeathManager : ILandOfDeathManager
{
    private readonly List<LandOfDeathInstance> _landOfDeathInstances = [];

    private readonly ConcurrentDictionary<(long, LandOfDeathMode), LandOfDeathInstance> _instancesByFamilyId = new();
    private readonly ConcurrentDictionary<long, LandOfDeathInstance> _instancesByPlayerId = new();
    private readonly ConcurrentDictionary<(long, LandOfDeathMode), LandOfDeathInstance> _instancesByGroupId = new();
    private readonly ConcurrentDictionary<LandOfDeathMode, LandOfDeathInstance> _instancesByPublic = new();

    public bool IsActive { get; set; }
    public bool IsDevilActive { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public DateTime NextDevilSpawn { get; set; }
    public DateTime? NextDevilDespawn { get; set; }

    public IReadOnlyCollection<LandOfDeathInstance> Instances => _landOfDeathInstances;

    public LandOfDeathInstance GetLandOfDeathInstanceByFamilyId(long familyId, LandOfDeathMode mode) 
        => _instancesByFamilyId.GetValueOrDefault((familyId, mode));

    public LandOfDeathInstance GetLandOfDeathInstanceByPlayerId(int playerId) 
        => _instancesByPlayerId.GetValueOrDefault(playerId);

    public LandOfDeathInstance GetLandOfDeathInstanceByGroupId(int groupId, LandOfDeathMode mode) 
        => _instancesByGroupId.GetValueOrDefault((groupId, mode));

    public LandOfDeathInstance GetLandOfDeathInstanceByPublic(LandOfDeathMode mode) 
        => _instancesByPublic.GetValueOrDefault(mode);

    public IEnumerable<LandOfDeathInstance> GetGroupLandOfDeathInstances() 
        => _instancesByGroupId.Values;

    public void AddLandOfDeathInstanceByFamily(long familyId, LandOfDeathMode mode, LandOfDeathInstance instance)
    {
        if (_instancesByFamilyId.TryAdd((familyId, mode), instance))
        {
            _landOfDeathInstances.Add(instance);
        }
    }

    public void AddLandOfDeathInstanceByPlayer(int playerId, LandOfDeathInstance instance)
    {
        if (_instancesByPlayerId.TryAdd(playerId, instance))
        {
            _landOfDeathInstances.Add(instance);
        }
    }

    public void AddLandOfDeathInstanceByGroup(int groupId, LandOfDeathMode mode, LandOfDeathInstance instance)
    {
        if (_instancesByGroupId.TryAdd((groupId, mode), instance))
        {
            _landOfDeathInstances.Add(instance);
        }
    }
    
    public void AddLandOfDeathInstanceByPublic(LandOfDeathMode mode, LandOfDeathInstance instance)
    {
        if (_instancesByPublic.TryAdd(mode, instance))
        {
            _landOfDeathInstances.Add(instance);
        }
    }
    
    public void RemoveLandOfDeathInstance(LandOfDeathInstance instance)
    {
        KeyValuePair<(long, LandOfDeathMode), LandOfDeathInstance> familyEntry = _instancesByFamilyId.FirstOrDefault(x => x.Value == instance);
        if (familyEntry.Value != null)
        {
            _instancesByFamilyId.TryRemove(familyEntry.Key, out _);
            _landOfDeathInstances.Remove(instance);
            return;
        }

        KeyValuePair<(long, LandOfDeathMode), LandOfDeathInstance> groupEntry = _instancesByGroupId.FirstOrDefault(x => x.Value == instance);
        if (groupEntry.Value != null)
        {
            _instancesByGroupId.TryRemove(groupEntry.Key, out _);
            _landOfDeathInstances.Remove(instance);
            return;
        }

        KeyValuePair<LandOfDeathMode, LandOfDeathInstance> publicEntry = _instancesByPublic.FirstOrDefault(x => x.Value == instance);
        if (publicEntry.Value != null)
        {
            _instancesByPublic.TryRemove(publicEntry.Key, out _);
            _landOfDeathInstances.Remove(instance);
        }
    }

    public void Clear()
    {
        _landOfDeathInstances.Clear();
        _instancesByFamilyId.Clear();
        _instancesByPlayerId.Clear();
        _instancesByGroupId.Clear();
        _instancesByPublic.Clear();
    }
}
