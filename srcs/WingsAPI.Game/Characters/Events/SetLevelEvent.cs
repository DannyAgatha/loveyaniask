using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class SetLevelEvent : PlayerEvent
{
    public LevelType LevelType { get; init; }
    public byte Level { get; init; }
}