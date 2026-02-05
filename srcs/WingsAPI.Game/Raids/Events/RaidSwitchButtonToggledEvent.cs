using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidSwitchButtonToggledEvent : PlayerEvent
{
    public long LeverId { get; init; }
}