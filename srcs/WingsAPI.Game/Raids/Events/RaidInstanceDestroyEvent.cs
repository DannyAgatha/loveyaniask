using PhoenixLib.Events;

namespace WingsEmu.Game.Raids.Events;

public class RaidInstanceDestroyEvent : IAsyncEvent
{
    public RaidInstanceDestroyEvent(RaidParty raidParty, bool isMarathonMode = false)
    {
        RaidParty = raidParty;
        IsMarathonMode = isMarathonMode;
    }

    public RaidParty RaidParty { get; }
    public bool IsMarathonMode { get; }
}