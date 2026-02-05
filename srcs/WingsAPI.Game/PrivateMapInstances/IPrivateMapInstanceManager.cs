using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.PrivateMapInstances;

public interface IPrivateMapInstanceManager
{
    IReadOnlyCollection<PrivateMapInstance> Instances { get; }
    
    PrivateMapInstance GetByFamilyId(long familyId);
    PrivateMapInstance GetByPlayerId(int playerId);
    PrivateMapInstance GetByGroupId(int groupId);
    PrivateMapInstance GetByMapVnum(int mapVnum);
    
    void AddByFamily(long familyId, PrivateMapInstance instance);
    void AddByPlayer(int playerId, PrivateMapInstance instance);
    void AddByGroup(int groupId, PrivateMapInstance instance);
    void AddByMapVnum(int mapVnum, PrivateMapInstance instance);
    
    void RemoveByFamily(long familyId);
    void RemoveByPlayer(int playerId);
    void RemoveByGroup(int groupId);
    void RemoveByMapVnum(int mapVnum);
}

public class PrivateMapInstanceManager : IPrivateMapInstanceManager
{
    private readonly ConcurrentDictionary<Guid, PrivateMapInstance> _instances = new();
    private readonly ConcurrentDictionary<long, Guid> _instancesByFamilyId = new();
    private readonly ConcurrentDictionary<int, Guid> _instancesByPlayerId = new();
    private readonly ConcurrentDictionary<int, Guid> _instancesByGroupId = new();
    private readonly ConcurrentDictionary<int, Guid> _instancesByMapVnum = new();

    public IReadOnlyCollection<PrivateMapInstance> Instances => _instances.Values.ToArray();

    public PrivateMapInstance GetByFamilyId(long familyId) => !_instancesByFamilyId.TryGetValue(familyId, out Guid id) ? null : _instances.GetValueOrDefault(id);

    public PrivateMapInstance GetByPlayerId(int playerId) => !_instancesByPlayerId.TryGetValue(playerId, out Guid id) ? null : _instances.GetValueOrDefault(id);

    public PrivateMapInstance GetByGroupId(int groupId) => !_instancesByGroupId.TryGetValue(groupId, out Guid id) ? null : _instances.GetValueOrDefault(id);
    public PrivateMapInstance GetByMapVnum(int mapVnum) => !_instancesByMapVnum.TryGetValue(mapVnum, out Guid id) ? null : _instances.GetValueOrDefault(id);

    public void AddByFamily(long familyId, PrivateMapInstance instance)
    {
        _instancesByFamilyId.TryAdd(familyId, instance.Id);
        _instances.TryAdd(instance.Id, instance);
    }

    public void AddByPlayer(int playerId, PrivateMapInstance instance)
    {
        _instancesByPlayerId.TryAdd(playerId, instance.Id);
        _instances.TryAdd(instance.Id, instance);
    }

    public void AddByGroup(int groupId, PrivateMapInstance instance)
    {
        _instancesByGroupId.TryAdd(groupId, instance.Id);
        _instances.TryAdd(instance.Id, instance);
    }

    public void AddByMapVnum(int mapVnum, PrivateMapInstance instance)
    {
        _instancesByMapVnum.TryAdd(mapVnum, instance.Id);
        _instances.TryAdd(instance.Id, instance);
    }

    public void RemoveByFamily(long familyId)
    {
        if (!_instancesByFamilyId.TryGetValue(familyId, out Guid id))
        {
            return;
        }

        _instancesByFamilyId.TryRemove(familyId, out _);
        _instances.TryRemove(id, out _);
    }

    public void RemoveByPlayer(int playerId)
    {
        if (!_instancesByPlayerId.TryGetValue(playerId, out Guid id))
        {
            return;
        }

        _instancesByPlayerId.TryRemove(playerId, out _);
        _instances.TryRemove(id, out _);
    }

    public void RemoveByGroup(int groupId)
    {
        if (!_instancesByGroupId.TryGetValue(groupId, out Guid id))
        {
            return;
        }

        _instancesByGroupId.TryRemove(groupId, out _);
        _instances.TryRemove(id, out _);
    }

    public void RemoveByMapVnum(int mapVnum)
    {
        if (!_instancesByMapVnum.TryGetValue(mapVnum, out Guid id))
        {
            return;
        }

        _instancesByMapVnum.TryRemove(mapVnum, out _);
        _instances.TryRemove(id, out _);
    }
}