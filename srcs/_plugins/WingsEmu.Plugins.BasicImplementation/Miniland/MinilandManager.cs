using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Factories;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Logging;
using WingsAPI.Data.Miniland;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Miniland;

public class MinilandManager : IMinilandManager
{
    private readonly IExpirableLockService _expirableLock;
    private readonly IMapDesignObjectFactory _mapDesignObjectFactory;
    private readonly IMapManager _mapManager;
    private readonly IMinigameManager _minigameManager;
    private readonly Dictionary<long, int> _minilandCapacities = new();
    private readonly MinilandConfiguration _minilandConfiguration;
    private readonly ConcurrentDictionary<long, IMapInstance> _minilandInstances = new();
    private readonly Dictionary<long, List<long>> _minilandInvitations = new();
    private readonly ISessionManager _sessionManager;

    public MinilandManager(IMapManager mapManager, IMapDesignObjectFactory mapDesignObjectFactory, ISessionManager sessionManager,
        IMinigameManager minigameManager, MinilandConfiguration minilandConfiguration, IExpirableLockService expirableLock)
    {
        _mapManager = mapManager;
        _mapDesignObjectFactory = mapDesignObjectFactory;
        _sessionManager = sessionManager;
        _minigameManager = minigameManager;
        _minilandConfiguration = minilandConfiguration;
        _expirableLock = expirableLock;
    }

    public async Task IncreaseMinilandVisitCounter(long characterId)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);
        await _expirableLock.TryIncrementTemporaryLockCounter($"game:locks:miniland-visit-counter:{characterId}", int.MaxValue, DateTime.UtcNow.Date.AddDays(1));
        session.PlayerEntity.LifetimeStats.TotalMinilandVisits++;
    }

    public async Task<bool> CanRefreshDailyVisitCounter(long characterId) =>
        await _expirableLock.TryAddTemporaryLockAsync($"game:locks:miniland-visit-counter:{characterId}", DateTime.UtcNow.Date.AddDays(1));

    public async Task<int> GetMinilandVisitCounter(long characterId)
    {
        (bool exists, int counter) = await _expirableLock.TryGetTemporaryCounterValue($"game:locks:miniland-visit-counter:{characterId}");
        return counter;
    }

    public IMapInstance CreateMinilandByCharacterSession(IClientSession session)
    {
        _minigameManager.RegisterInteraction(session, new MinilandInteractionInformationHolder(MinigameInteraction.None, default));

        IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId((int)MapIds.MINILAND, MapInstanceType.Miniland);

        WingsEmu.Game.Configurations.Miniland.Miniland minilandConfiguration = GetMinilandConfiguration(mapInstance);

        SetMinilandCapacity(session.PlayerEntity.Id, minilandConfiguration.DefaultMaximumCapacity);

        // It will remove object if somehow player doesn't have an item in inventory or the object just disappeared
        List<CharacterMinilandObjectDto> toRemove = new();
        foreach (CharacterMinilandObjectDto obj in session.PlayerEntity.MinilandObjects)
        {
            try
            {
                MapDesignObject mapObject = _mapDesignObjectFactory.CreateGameObject(session.PlayerEntity.Id, obj);
                if (mapObject == null)
                {
                    toRemove.Add(obj);
                    continue;
                }

                session.EmitEventAsync(new AddObjMinilandEndLogicEvent(mapObject, mapInstance));
            }
            catch (Exception e)
            {
                Log.Error("[MINILAND_MANAGER] Couldn't create map design object", e);
            }
        }

        foreach (CharacterMinilandObjectDto obj in toRemove)
        {
            session.PlayerEntity.MinilandObjects.Remove(obj);
        }

        return !_minilandInstances.TryAdd(session.PlayerEntity.Id, mapInstance) ? null : mapInstance;
    }

    public IMapInstance GetMinilandByCharacterId(long characterId) => _minilandInstances.GetValueOrDefault(characterId);

    public IClientSession GetSessionByMiniland(IMapInstance mapInstance)
    {
        foreach ((long characterId, IMapInstance instance) in _minilandInstances)
        {
            if (instance.Id == mapInstance.Id)
            {
                return _sessionManager.GetSessionByCharacterId(characterId);
            }
        }

        return default;
    }

    public void SaveMinilandInvite(long senderId, long targetId)
    {
        if (!_minilandInvitations.TryGetValue(senderId, out List<long> invites))
        {
            invites = new List<long>();
            _minilandInvitations[senderId] = invites;
        }

        invites.Add(targetId);
    }

    public bool ContainsMinilandInvite(long senderId) => _minilandInvitations.ContainsKey(senderId);

    public bool ContainsTargetInvite(long senderId, long targetId) => _minilandInvitations.TryGetValue(senderId, out List<long> targetList) && targetList.Contains(targetId);

    public void RemoveMinilandInvite(long senderId, long targetId)
    {
        if (!_minilandInvitations.TryGetValue(senderId, out List<long> invites))
        {
            invites = new List<long>();
        }

        invites.Remove(targetId);
    }

    public int GetMinilandMaximumCapacity(long characterId) => _minilandCapacities.TryGetValue(characterId, out int capacity) ? capacity : default;

    public void RelativeUpdateMinilandCapacity(long characterId, int valueToAdd)
    {
        if (_minilandCapacities.TryAdd(characterId, valueToAdd))
        {
            return;
        }

        _minilandCapacities[characterId] += valueToAdd;
    }

    public WingsEmu.Game.Configurations.Miniland.Miniland GetMinilandConfiguration(IMapInstance mapInstance)
    {
        if (mapInstance == null)
        {
            return null;
        }

        WingsEmu.Game.Configurations.Miniland.Miniland minilandConfiguration = _minilandConfiguration.FirstOrDefault(x => x.MapVnum == mapInstance.MapId);

        return minilandConfiguration;
    }

    public void RemoveMiniland(long characterId)
    {
        if (!_minilandInstances.TryGetValue(characterId, out IMapInstance miniland))
        {
            return;
        }

        _minilandInstances.TryRemove(characterId, out _);
        _mapManager.RemoveMapInstance(miniland.Id);
        miniland.Destroy();
    }

    private void SetMinilandCapacity(long characterId, int value)
    {
        if (_minilandCapacities.TryAdd(characterId, value))
        {
            return;
        }

        _minilandCapacities[characterId] = value;
    }
}