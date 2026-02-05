using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4.Event;

public class Act4FactionPointsIncreaseEvent : PlayerEvent
{
    public Act4FactionPointsIncreaseEvent(int pointsToAdd) => PointsToAdd = pointsToAdd;

    public Act4FactionPointsIncreaseEvent(FactionType factionType, int pointsToAdd)
    {
        PreferedFactionType = factionType;
        PointsToAdd = pointsToAdd;
    }

    private FactionType? PreferedFactionType { get; }
    public int PointsToAdd { get; }

    public FactionType FactionType => PreferedFactionType ?? Sender.PlayerEntity.Faction;
}