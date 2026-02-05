using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Families.Enum;

namespace WingsEmu.Game.Families.Event;

public class FamilyAddExperienceEvent : PlayerEvent
{
    public FamilyAddExperienceEvent(long experienceGained, FamXpObtainedFromType famXpObtainedFromType)
    {
        ExperienceGained = experienceGained;
        FamXpObtainedFromType = famXpObtainedFromType;
    }

    public long ExperienceGained { get; }
    public FamXpObtainedFromType FamXpObtainedFromType { get; }
}