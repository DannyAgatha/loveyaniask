using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyChangeDeputyEvent : PlayerEvent
{
    public FamilyChangeDeputyEvent(string sourceName, string targetName)
    {
        SourceName = sourceName;
        TargetName = targetName;
    }

    public string SourceName { get; }
    public string TargetName { get; }
}