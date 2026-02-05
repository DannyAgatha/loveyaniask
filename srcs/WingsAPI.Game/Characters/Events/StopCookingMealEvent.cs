using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events
{
    public class StopCookingMealEvent : PlayerEvent
    {
        public StopCookingMealEvent(bool sendEffsPacket) => SendEffsPacket = sendEffsPacket;
        public bool SendEffsPacket { get; set; }
    }
}