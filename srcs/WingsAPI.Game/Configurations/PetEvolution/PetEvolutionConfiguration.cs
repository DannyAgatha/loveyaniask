using System.Collections.Generic;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations.PetEvolution;

public class PetEvolutionConfiguration
{
    public List<PetEvolutionDefinition> PetEvolutions { get; init; } = [];
}

public class PetEvolutionDefinition
{
    public required int OriginalMonsterVnum { get; init; }
    public required byte EvolveAtLevel { get; init; }
    public required PetLevelType LevelType { get; init; }
    public required int EvolvedMonsterVnum { get; init; }
}