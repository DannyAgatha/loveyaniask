using PhoenixLib.Events;
using WingsEmu.Game.Raids.Enum;

namespace WingsEmu.Game.Raids.Events;

public class RaidInstanceFinishEvent : IAsyncEvent
{
    public RaidInstanceFinishEvent(RaidParty raidParty, RaidFinishType raidFinishType)
    {
        RaidFinishType = raidFinishType;
        RaidParty = raidParty;
    }

    public RaidParty RaidParty { get; }

    public RaidFinishType RaidFinishType { get; }
}