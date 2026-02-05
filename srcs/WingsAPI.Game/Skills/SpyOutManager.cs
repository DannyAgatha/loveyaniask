using System.Collections.Generic;
using WingsEmu.Core.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Skills;

public class SpyOutManager : ISpyOutManager
{
    private readonly Dictionary<long, (long id, VisualType type)> _spyOut = new();

    public void AddSpyOutSkill(long characterId, long targetId, VisualType targetType)
    {
        _spyOut[characterId] = (targetId, targetType);
    }

    public bool ContainsSpyOut(long characterId) => _spyOut.ContainsKey(characterId);

    public (long targetId, VisualType targetType) GetSpyOutTarget(long characterId) => _spyOut.GetOrDefault(characterId);

    public void RemoveSpyOutSkill(long characterId)
    {
        if (!ContainsSpyOut(characterId))
        {
            return;
        }

        _spyOut.Remove(characterId);
    }
}