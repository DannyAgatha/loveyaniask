using System.Collections.Concurrent;
using System.Collections.Generic;
using WingsEmu.Core.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Battle;

public class SkillUsageManager : ISkillUsageManager
{
    private readonly ConcurrentDictionary<long, ComboState> _combos = new();
    private readonly ConcurrentDictionary<long, List<(VisualType, long)>> _multiTargets = new();

    public List<(VisualType, long)> GetMultiTargets(long id) => _multiTargets.GetOrDefault(id, new List<(VisualType, long)>());

    public void SetMultiTargets(long id, List<(VisualType, long)> targets)
    {
        _multiTargets[id] = targets;
    }

    public void ResetMultiTargets(long id)
    {
        _multiTargets.TryRemove(id, out List<(VisualType, long)> targets);
    }

    public ComboState GetComboState(long casterId, long targetId)
    {
        ComboState tmp = _combos.GetOrAdd(casterId, new ComboState(targetId));
        if (tmp.TargetId == targetId)
        {
            return tmp;
        }

        tmp = new ComboState(targetId);
        _combos[casterId] = tmp;

        return tmp;
    }

    public void ResetComboState(long id)
    {
        _combos.TryRemove(id, out ComboState state);
    }
}