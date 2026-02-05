using System.Collections.Generic;

namespace WingsEmu.Game.GameEvent;

public interface IGameEventInstanceManager
{
    IReadOnlyCollection<IGameEventInstance> GetGameEventsByType(GameEventType gameEventType);
    void AddGameEvent(IGameEventInstance gameEventInstance);
    void RemoveGameEvent(IGameEventInstance gameEventInstance);
}