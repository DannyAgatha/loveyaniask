using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Revival;

public class RevivalAskEvent : PlayerEvent
{
    public RevivalAskEvent(AskRevivalType askRevivalType) => AskRevivalType = askRevivalType;

    public AskRevivalType AskRevivalType { get; }
}

public enum AskRevivalType
{
    BasicRevival,
    ArenaRevival,
    DungeonRevival,
    TimeSpaceRevival,
    LandOfDeathRevival,
    LandOfLifeRevival,
    CaligorRevival
}