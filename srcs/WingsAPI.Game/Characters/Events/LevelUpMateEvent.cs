using WingsAPI.Packets.Enums;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Characters.Events;

public class LevelUpMateEvent : PlayerEvent
{
    public byte Level { get; init; }
    public int NosMateMonsterVnum { get; init; }
    public MateLevelUpType LevelUpType { get; init; }
    public Location Location { get; init; }
    public int? ItemVnum { get; init; }
}