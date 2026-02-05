using System.Collections.Generic;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Event.Global
{
    public class GameEventMatchmakeEvent : GameEvent
    {
        public GameEventMatchmakeEvent(GameEventType gameEventType, List<IClientSession> sessions) : base(gameEventType) => Sessions = sessions;

        public List<IClientSession> Sessions { get; }
    }
}