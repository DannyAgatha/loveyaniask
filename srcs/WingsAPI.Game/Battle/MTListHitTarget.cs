// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Battle;

public class MTListHitTarget
{
    #region Instantiation

    public MTListHitTarget(VisualType entityType, long targetId)
    {
        EntityType = entityType;
        TargetId = targetId;
    }

    #endregion


    #region Properties

    public VisualType EntityType { get; set; }

    public long TargetId { get; set; }

    #endregion
}