using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class ChangeFactionEvent : PlayerEvent
{
    public FactionType NewFaction { get; init; }
}