namespace WingsEmu.Game.Configurations.Miniland;

public class RestrictedZone
{
    public RestrictionType RestrictionTag { get; set; }

    public SerializablePosition Corner1 { get; set; } = new();

    public SerializablePosition Corner2 { get; set; } = new();
}