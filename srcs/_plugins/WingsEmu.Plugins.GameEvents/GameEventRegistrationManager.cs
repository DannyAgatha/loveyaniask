using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WingsEmu.Core.Generics;
using WingsEmu.Game.GameEvent;

namespace WingsEmu.Plugins.GameEvents
{
    public class GameEventRegistrationManager : IGameEventRegistrationManager
    {
        private readonly ConcurrentDictionary<GameEventType, ThreadSafeHashSet<long>> _characterIdsByGameEventInclination = new();
        private readonly ConcurrentDictionary<GameEventType, DateTime> _gameEventRegistrations = new();
        private readonly ConcurrentDictionary<long, GameEventType> _gameEventTypeByCharacterId = new();

        public IReadOnlyCollection<KeyValuePair<GameEventType, DateTime>> GameEventRegistrations => _gameEventRegistrations;

        public bool AddGameEventRegistration(GameEventType gameEventType, DateTime currentTime, DateTime expiryDate)
        {
            if (_gameEventRegistrations.TryGetValue(gameEventType, out DateTime unlockDate))
            {
                if (unlockDate < currentTime)
                {
                    return false;
                }
            }

            _gameEventRegistrations[gameEventType] = expiryDate;
            return true;
        }

        public bool IsGameEventRegistrationOpen(GameEventType gameEventType, DateTime currentTime) =>
            _gameEventRegistrations.TryGetValue(gameEventType, out DateTime unlockDate) && currentTime < unlockDate;

        public void RemoveGameEventRegistration(GameEventType gameEventType)
        {
            _gameEventRegistrations[gameEventType] = DateTime.MaxValue;
        }

        public void SetCharacterGameEventInclination(long id, GameEventType gameEventType)
        {
            if (_gameEventTypeByCharacterId.TryGetValue(id, out GameEventType registeredGameEventType))
            {
                _characterIdsByGameEventInclination[registeredGameEventType].Remove(id);
            }

            if (_characterIdsByGameEventInclination.TryGetValue(gameEventType, out ThreadSafeHashSet<long> list))
            {
                list.Add(id);
            }
            else
            {
                _characterIdsByGameEventInclination.TryAdd(gameEventType, new ThreadSafeHashSet<long>
                {
                    id
                });
            }

            if (!_gameEventTypeByCharacterId.TryAdd(id, gameEventType))
            {
                _gameEventTypeByCharacterId[id] = gameEventType;
            }
        }

        public ThreadSafeHashSet<long> GetAndRemoveCharactersByGameEventInclination(GameEventType gameEventType)
        {
            ThreadSafeHashSet<long> valueToReturn = _characterIdsByGameEventInclination.TryGetValue(gameEventType, out ThreadSafeHashSet<long> characterIds) ? characterIds : null;
            RemoveCharactersByGameEventInclination(gameEventType, valueToReturn);
            return valueToReturn;
        }

        public void RemoveCharactersByGameEventInclination(GameEventType gameEventType)
        {
            RemoveCharactersByGameEventInclination(gameEventType, _characterIdsByGameEventInclination.TryGetValue(gameEventType, out ThreadSafeHashSet<long> characterIds) ? characterIds : null);
        }

        private void RemoveCharactersByGameEventInclination(GameEventType gameEventType, ThreadSafeHashSet<long> characters)
        {
            _characterIdsByGameEventInclination.TryRemove(gameEventType, out _);

            if (characters == null)
            {
                return;
            }

            foreach (long character in characters)
            {
                _gameEventTypeByCharacterId.TryRemove(character, out _);
            }
        }
    }
}