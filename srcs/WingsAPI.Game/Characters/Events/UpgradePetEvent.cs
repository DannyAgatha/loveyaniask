using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.Characters.Events;

public class UpgradePetEvent : PlayerEvent
{
    public IMateEntity Mate { get; set; }
}