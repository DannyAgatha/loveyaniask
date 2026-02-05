using System.Collections.Generic;
using WingsAPI.Packets.Enums.Character;

namespace WingsEmu.Game.Configurations.InitialConfiguration;

public class CharacterJobConfiguration
{
    public required byte Level { get; init; }
    public required byte JobLevel { get; init; }
    public required int Hp { get; init; }
    public required int Mp { get; init; }
    public required int Reput { get; init; }
    public required byte Dignity { get; init; }
    public required short MinilandPoints { get; init; }
}

public class InitialCharacterJobConfiguration
{
    public Dictionary<CharacterCreationClassOption, CharacterJobConfiguration> CharacterJobs { get; init; }
}