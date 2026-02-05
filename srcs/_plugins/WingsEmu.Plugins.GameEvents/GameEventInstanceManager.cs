using System.Collections.Generic;
using WingsEmu.Game.GameEvent;

namespace WingsEmu.Plugins.GameEvents
{
    public class GameEventInstanceManager : IGameEventInstanceManager
    {
        private readonly Dictionary<GameEventType, List<IGameEventInstance>> _gameEventInstances = new();

        public IReadOnlyCollection<IGameEventInstance> GetGameEventsByType(GameEventType gameEventType) =>
            _gameEventInstances.TryGetValue(gameEventType, out List<IGameEventInstance> gameEventInstance) ? gameEventInstance : null;

        public void AddGameEvent(IGameEventInstance gameEventInstance)
        {
            if (_gameEventInstances.TryGetValue(gameEventInstance.GameEventType, out List<IGameEventInstance> gameEventInstances))
            {
                gameEventInstances.Add(gameEventInstance);
                return;
            }

            _gameEventInstances[gameEventInstance.GameEventType] = new List<IGameEventInstance>
            {
                gameEventInstance
            };
        }

        public void RemoveGameEvent(IGameEventInstance gameEventInstance)
        {
            if (_gameEventInstances.TryGetValue(gameEventInstance.GameEventType, out List<IGameEventInstance> gameEventInstances))
            {
                gameEventInstances.Remove(gameEventInstance);
            }
        }
    }
}