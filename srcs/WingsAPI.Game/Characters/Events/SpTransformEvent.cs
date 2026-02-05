using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Characters.Events;

public class SpTransformEvent : PlayerEvent
{
    public GameItemInstance Specialist { get; set; }

    public bool Forced { get; set; }
}