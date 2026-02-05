using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyWarehouseLogsOpenEvent : PlayerEvent
{
    public bool Refresh { get; init; }
}