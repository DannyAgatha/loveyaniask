using WingsAPI.Packets.Enums;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class TrophyFragmentsEvent : PlayerEvent
{
    public RaidType RaidType { get; init; }
}