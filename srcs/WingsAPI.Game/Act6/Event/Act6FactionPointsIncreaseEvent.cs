using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Act6.Event
{
    public class Act6FactionPointsIncreaseEvent : PlayerEvent
    {
        public Act6FactionPointsIncreaseEvent(int pointsToAdd = 1) => PointsToAdd = pointsToAdd;

        public int PointsToAdd { get; }
    }
}