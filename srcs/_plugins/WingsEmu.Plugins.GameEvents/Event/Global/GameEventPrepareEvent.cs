using WingsEmu.Game.GameEvent;

namespace WingsEmu.Plugins.GameEvents.Event.Global
{
    public class GameEventPrepareEvent : GameEvent
    {
        public GameEventPrepareEvent(GameEventType type) : base(type)
        {
        }

        public bool NoDelay { get; set; }
    }
}