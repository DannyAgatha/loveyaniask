using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Skills;

public interface ISpyOutManager
{
    void AddSpyOutSkill(long characterId, long targetId, VisualType targetType);
    bool ContainsSpyOut(long characterId);
    (long targetId, VisualType targetType) GetSpyOutTarget(long characterId);
    void RemoveSpyOutSkill(long characterId);
}