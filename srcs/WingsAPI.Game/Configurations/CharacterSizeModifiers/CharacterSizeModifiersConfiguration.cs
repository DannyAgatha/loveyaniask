using System.Collections.Generic;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Configurations.CharacterSizeModifiers;

public class CharacterSizeModifiersConfiguration
{
    public List<SizeTypeDefinition> SizeTypes { get; init; } = [];
    public List<CharacterSizeModifierSet> Sets { get; init; } = [];
}

public class SizeTypeDefinition
{
    public required EntityType EntityType { get; init; }
    public required int DefaultSize { get; init; }
}

public class CharacterSizeModifierSet
{
    public required string SetName { get; init; }
    public required int ReducedSize { get; init; }
    public required List<int> HatVnums { get; init; }
    public required List<int> CostumeVnums { get; init; }
}