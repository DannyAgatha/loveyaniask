using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Revival;

public class RevivalReviveEvent : PlayerEvent
{
    public RevivalReviveEvent()
    {
    }

    public RevivalReviveEvent(RevivalType revivalType) => RevivalType = revivalType;

    public RevivalReviveEvent(RevivalType revivalType, ForcedType forced)
    {
        RevivalType = revivalType;
        Forced = forced;
    }

    public RevivalType RevivalType { get; set; }

    public ForcedType Forced { get; set; }
}

public enum RevivalType
{
    TryPayRevival = 0,
    DontPayRevival = 1,
    TryPayArenaRevival = 2
}

public enum ForcedType
{
    NoForced,
    Forced,
    Reconnect,
    Act4SealRevival,
    HolyRevival
}