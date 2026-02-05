using WingsEmu.Game._enum;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TrainerSpecialist;

public class UpdateTrainerQuestEvent : PlayerEvent
{
    public PetTrainerMissionType MissionType { get; set; }
}
