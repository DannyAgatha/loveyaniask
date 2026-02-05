using PhoenixLib.Events;
using WingsEmu.Game.GameEvent;

namespace WingsEmu.Plugins.GameEvents.Event.Global
{
    public abstract class GameEvent : IAsyncEvent
    {
        protected GameEvent(GameEventType type) => Type = type;

        public GameEventType Type { get; }
    }
}