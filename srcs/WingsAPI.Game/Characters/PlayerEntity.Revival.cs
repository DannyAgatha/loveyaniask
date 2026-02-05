// NosEmu
// 


using System;
using WingsEmu.Game.Revival;

namespace WingsEmu.Game.Characters;

/// <summary>
///     Revival
/// </summary>
public partial class PlayerEntity
{
    private readonly CharacterRevivalComponent _characterRevivalComponent;

    public DateTime RevivalDateTimeForExecution => _characterRevivalComponent.RevivalDateTimeForExecution;

    public RevivalType RevivalType => _characterRevivalComponent.RevivalType;

    public ForcedType ForcedType => _characterRevivalComponent.ForcedType;

    public DateTime AskRevivalDateTimeForExecution => _characterRevivalComponent.AskRevivalDateTimeForExecution;

    public AskRevivalType AskRevivalType => _characterRevivalComponent.AskRevivalType;

    public void UpdateRevival(DateTime revivalDateTimeForExecution, RevivalType revivalType, ForcedType forcedType)
    {
        _characterRevivalComponent.UpdateRevival(revivalDateTimeForExecution, revivalType, forcedType);
    }

    public void DisableRevival()
    {
        _characterRevivalComponent.DisableRevival();
    }

    public void UpdateAskRevival(DateTime askRevivalDateTimeForExecution, AskRevivalType askRevivalType)
    {
        _characterRevivalComponent.UpdateAskRevival(askRevivalDateTimeForExecution, askRevivalType);
    }

    public void DisableAskRevival()
    {
        _characterRevivalComponent.DisableAskRevival();
    }
}