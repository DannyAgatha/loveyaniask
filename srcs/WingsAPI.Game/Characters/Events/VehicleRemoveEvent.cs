using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class RemoveVehicleEvent : PlayerEvent
{
    public RemoveVehicleEvent(bool showMates = false) => ShowMates = showMates;

    public bool ShowMates { get; }
}