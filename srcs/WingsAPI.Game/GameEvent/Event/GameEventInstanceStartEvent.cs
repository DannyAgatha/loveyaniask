using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.GameEvent.Event;

public class GameEventInstanceStartEvent : IAsyncEvent
{
    public GameEventInstanceStartEvent(IEnumerable<IClientSession> sessions, IGameEventConfiguration gameEventConfiguration)
    {
        Sessions = sessions;
        GameEventConfiguration = gameEventConfiguration;
    }

    public IEnumerable<IClientSession> Sessions { get; }

    public IGameEventConfiguration GameEventConfiguration { get; }
}