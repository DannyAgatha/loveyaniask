using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.TrainerSpecialist;

public class UpdatePetBookEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; set; }
}
