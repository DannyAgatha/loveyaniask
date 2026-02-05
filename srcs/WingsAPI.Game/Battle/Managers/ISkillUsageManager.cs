using System.Collections.Generic;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Battle;

public interface ISkillUsageManager
{
    List<(VisualType, long)> GetMultiTargets(long id);
    void SetMultiTargets(long id, List<(VisualType, long)> targets);
    void ResetMultiTargets(long id);

    ComboState GetComboState(long casterId, long targetId);
    void ResetComboState(long id);
}