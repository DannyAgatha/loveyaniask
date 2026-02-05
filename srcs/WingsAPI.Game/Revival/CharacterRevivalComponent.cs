using System;

namespace WingsEmu.Game.Revival;

public interface ICharacterRevivalComponent
{
    public DateTime RevivalDateTimeForExecution { get; }
    public RevivalType RevivalType { get; }
    public ForcedType ForcedType { get; }
    public DateTime AskRevivalDateTimeForExecution { get; }
    public AskRevivalType AskRevivalType { get; }
    public void UpdateRevival(DateTime revivalDateTimeForExecution, RevivalType revivalType, ForcedType forcedType);
    public void DisableRevival();
    public void UpdateAskRevival(DateTime askRevivalDateTimeForExecution, AskRevivalType askRevivalType);
    public void DisableAskRevival();
}

public class CharacterRevivalComponent : ICharacterRevivalComponent
{
    public DateTime RevivalDateTimeForExecution { get; private set; } = DateTime.MaxValue;

    public RevivalType RevivalType { get; private set; }

    public ForcedType ForcedType { get; private set; }

    public DateTime AskRevivalDateTimeForExecution { get; private set; } = DateTime.MaxValue;

    public AskRevivalType AskRevivalType { get; private set; }

    public void UpdateRevival(DateTime revivalDateTimeForExecution, RevivalType revivalType, ForcedType forcedType)
    {
        RevivalDateTimeForExecution = revivalDateTimeForExecution;
        RevivalType = revivalType;
        ForcedType = forcedType;
    }

    public void DisableRevival()
    {
        RevivalDateTimeForExecution = DateTime.MaxValue;
    }

    public void UpdateAskRevival(DateTime askRevivalDateTimeForExecution, AskRevivalType askRevivalType)
    {
        AskRevivalDateTimeForExecution = askRevivalDateTimeForExecution;
        AskRevivalType = askRevivalType;
    }

    public void DisableAskRevival()
    {
        AskRevivalDateTimeForExecution = DateTime.MaxValue;
    }
}