using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidPartyLeaveEvent : PlayerEvent
{
    public RaidPartyLeaveEvent(bool byKick, bool removeLife = true)
    {
        ByKick = byKick;
        RemoveLife = removeLife;
    }

    public bool ByKick { get; }

    public bool RemoveLife { get; }
}