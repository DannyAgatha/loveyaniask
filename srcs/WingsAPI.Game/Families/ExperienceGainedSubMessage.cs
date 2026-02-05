using System;
using WingsEmu.Game.Families.Enum;

namespace WingsEmu.Game.Families;

public class ExperienceGainedSubMessage
{
    public ExperienceGainedSubMessage(long characterId, long famXpGained, FamXpObtainedFromType famXpGainedFromType, DateTime dateOfReward)
    {
        CharacterId = characterId;
        FamXpGained = famXpGained;
        FamXpGainedFromType = famXpGainedFromType;
        DateOfReward = dateOfReward;
    }

    public long CharacterId { get; }

    public long FamXpGained { get; }

    public FamXpObtainedFromType FamXpGainedFromType { get; }

    public DateTime DateOfReward { get; }
}