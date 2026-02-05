using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceTryStartHiddenEvent : PlayerEvent
{
    public INpcEntity TimeSpacePortal { get; init; }
    public bool IsChallengeMode { get; init; }
}