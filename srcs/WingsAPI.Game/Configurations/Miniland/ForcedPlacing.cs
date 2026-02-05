using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations.Miniland;

public class ForcedPlacing
{
    public MinilandItemSubType SubType { get; set; } = MinilandItemSubType.HOUSE;

    public SerializablePosition ForcedLocation { get; set; } = new();
}