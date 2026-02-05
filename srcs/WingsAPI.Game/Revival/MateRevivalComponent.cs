using System;

namespace WingsEmu.Game.Revival;

public interface IMateRevivalComponent
{
    DateTime RevivalDateTimeForExecution { get; }
    bool IsRevivalDelayed { get; }
    void UpdateRevival(DateTime revivalDateTimeForExecution, bool isRevivalDelayed);
    void DisableRevival();
}

public class MateRevivalComponent : IMateRevivalComponent
{
    public DateTime RevivalDateTimeForExecution { get; private set; } = DateTime.MaxValue;
    public bool IsRevivalDelayed { get; private set; }

    public void UpdateRevival(DateTime revivalDateTimeForExecution, bool isRevivalDelayed)
    {
        RevivalDateTimeForExecution = revivalDateTimeForExecution;
        IsRevivalDelayed = isRevivalDelayed;
    }

    public void DisableRevival()
    {
        RevivalDateTimeForExecution = DateTime.MaxValue;
    }
}