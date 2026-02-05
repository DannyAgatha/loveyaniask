using WingsEmu.Game.GameEvent;

namespace WingsEmu.Plugins.GameEvents.Event.Global
{
    public class GameEventLockRegistrationEvent : GameEvent
    {
        public GameEventLockRegistrationEvent(GameEventType type) : base(type)
        {
        }
    }
}