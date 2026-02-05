using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class DelayManager : IDelayManager
{
    private readonly IDelayConfiguration _configuration;
    private readonly ConcurrentDictionary<long, DelayedAction> _playerActions = new();

    public DelayManager(IDelayConfiguration configuration) => _configuration = configuration;

    public ValueTask<DateTime> RegisterAction(IBattleEntity entity, DelayedActionType actionType, TimeSpan time = default)
    {
        if (_playerActions.TryGetValue(entity.Id, out DelayedAction action))
        {
            CompleteAction(entity, action.Type);
        }

        var delayedAction = new DelayedAction
        {
            Type = actionType,
            Completion = DateTime.UtcNow.Add(time != default ? time : _configuration.GetDelayByAction(actionType)),
            MapId = entity.MapInstance.MapId,
            PositionX = entity.PositionX,
            PositionY = entity.PositionY
        };

        _playerActions.TryAdd(entity.Id, delayedAction);

        return new ValueTask<DateTime>(delayedAction.Completion);
    }

    public ValueTask<bool> CanPerformAction(IBattleEntity entity, DelayedActionType actionType)
    {
        DelayedAction action = _playerActions.GetOrDefault(entity.Id);
        if (action == null || action.Type != actionType)
        {
            return new ValueTask<bool>(false);
        }

        return new ValueTask<bool>(action.Completion <= DateTime.UtcNow && entity.MapInstance.MapId == action.MapId && entity.PositionX == action.PositionX && entity.PositionY == action.PositionY);
    }

    public ValueTask<bool> CompleteAction(IBattleEntity entity, DelayedActionType actionType)
    {
        DelayedAction action = _playerActions.GetOrDefault(entity.Id);
        if (action == null || action.Type != actionType)
        {
            return new ValueTask<bool>(false);
        }

        return new ValueTask<bool>(_playerActions.TryRemove(entity.Id, out DelayedAction delayedAction));
    }

    public ValueTask<int> RemoveAllOutdatedActions(TimeSpan time)
    {
        var keys = _playerActions.Where(x => x.Value.Completion.Add(time) < DateTime.UtcNow).Select(x => x.Key).ToList();

        foreach (long key in keys)
        {
            _playerActions.TryRemove(key, out DelayedAction action);
        }

        return new ValueTask<int>(keys.Count);
    }

    private class DelayedAction
    {
        public DelayedActionType Type { get; set; }
        public DateTime Completion { get; set; }
        public int MapId { get; set; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }
    }
}