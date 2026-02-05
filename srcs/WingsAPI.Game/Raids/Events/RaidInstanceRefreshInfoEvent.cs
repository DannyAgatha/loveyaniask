using PhoenixLib.Events;

namespace WingsEmu.Game.Raids.Events;

public class RaidInstanceRefreshInfoEvent : IAsyncEvent
{
    public RaidInstanceRefreshInfoEvent(RaidParty raidParty) => RaidParty = raidParty;

    public RaidParty RaidParty { get; }
}