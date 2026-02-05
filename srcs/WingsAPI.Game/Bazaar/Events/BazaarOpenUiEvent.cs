using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarOpenUiEvent : PlayerEvent
{
    public BazaarOpenUiEvent(bool throughMedal) => ThroughMedal = throughMedal;

    public bool ThroughMedal { get; }
}