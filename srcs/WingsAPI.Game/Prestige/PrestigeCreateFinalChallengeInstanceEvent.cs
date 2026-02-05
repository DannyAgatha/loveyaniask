using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Configurations.Prestige;

namespace WingsEmu.Game.Prestige;

public class PrestigeCreateFinalChallengeInstanceEvent : PlayerEvent
{
    public PrestigeFinalChallenge FinalChallenge { get; init; }
}