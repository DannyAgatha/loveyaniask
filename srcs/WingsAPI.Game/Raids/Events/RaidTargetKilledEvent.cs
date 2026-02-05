using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidTargetKilledEvent : PlayerEvent
{
    public long[] DamagerCharactersIds { get; init; }
}