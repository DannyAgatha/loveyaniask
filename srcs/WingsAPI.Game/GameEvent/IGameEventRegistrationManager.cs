using System;
using System.Collections.Generic;
using WingsEmu.Core.Generics;

namespace WingsEmu.Game.GameEvent;

public interface IGameEventRegistrationManager
{
    IReadOnlyCollection<KeyValuePair<GameEventType, DateTime>> GameEventRegistrations { get; }

    /// <summary>
    ///     Tries to add a registration, in the case of failing it will return false.
    /// </summary>
    /// <param name="gameEventType"></param>
    /// <param name="currentTime"></param>
    /// <param name="expiryDate"></param>
    /// <returns></returns>
    bool AddGameEventRegistration(GameEventType gameEventType, DateTime currentTime, DateTime expiryDate);

    void RemoveGameEventRegistration(GameEventType gameEventType);
    bool IsGameEventRegistrationOpen(GameEventType gameEventType, DateTime currentTime);

    void SetCharacterGameEventInclination(long id, GameEventType gameEventType);
    ThreadSafeHashSet<long> GetAndRemoveCharactersByGameEventInclination(GameEventType gameEventType);
    void RemoveCharactersByGameEventInclination(GameEventType gameEventType);
}