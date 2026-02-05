using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events
{
    public  class IncreaseFoodValueEvent : PlayerEvent
    {
        public IncreaseFoodValueEvent(int value) => Value = value;

        public int Value { get; set; }
    }
}