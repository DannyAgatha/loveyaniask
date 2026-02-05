using PhoenixLib.Events;

namespace WingsEmu.Game.Raids.Events;

public class RaidObjectiveIncreaseEvent : IAsyncEvent
{
    public RaidObjectiveIncreaseEvent(RaidTargetType raidTargetType, RaidSubInstance raidSubInstance)
    {
        RaidTargetType = raidTargetType;
        RaidSubInstance = raidSubInstance;
    }

    public RaidTargetType RaidTargetType { get; }

    public RaidSubInstance RaidSubInstance { get; }
}

public enum RaidTargetType
{
    Monster,
    Button
}