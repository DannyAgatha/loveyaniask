using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class LevelUpEvent : PlayerEvent
{
    public LevelType LevelType { get; init; }
    public int? ItemVnum { get; init; }
}