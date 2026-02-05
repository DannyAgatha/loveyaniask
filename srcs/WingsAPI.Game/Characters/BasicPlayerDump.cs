using WingsEmu.DTOs.Items;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Characters;

public class BasicPlayerDump
{
    public long CharacterId { get; init; }
    public int Level { get; init; }
    public ClassType Class { get; init; }
    public ItemInstanceDTO Specialist { get; init; }
    public int TotalFireResistance { get; init; }
    public int TotalWaterResistance { get; init; }
    public int TotalLightResistance { get; init; }
    public int TotalDarkResistance { get; init; }
    public int? FairyLevel { get; init; }
}