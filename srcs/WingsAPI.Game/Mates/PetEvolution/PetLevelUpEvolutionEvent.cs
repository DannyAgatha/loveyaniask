using WingsEmu.Game._enum;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Mates.PetEvolution;

public class PetLevelUpEvolutionEvent : PlayerEvent
{
    public byte Level { get; init; }
    public int NosMateMonsterVnum { get; init; }
    public PetLevelType LevelType { get; init; }
    public Location Location { get; init; }
    public int? ItemVnum { get; init; }
}