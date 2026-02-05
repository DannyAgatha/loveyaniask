using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events
{
    public class DecreaseFoodValueEvent : PlayerEvent
    {
        public DecreaseFoodValueEvent(int value) => Value = value;

        public int Value { get; set; }
    }
}