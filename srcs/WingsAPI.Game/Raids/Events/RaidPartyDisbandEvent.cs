using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidPartyDisbandEvent : PlayerEvent
{
    public RaidPartyDisbandEvent(bool isByRdPacket = false) => IsByRdPacket = isByRdPacket;

    public bool IsByRdPacket { get; }
}