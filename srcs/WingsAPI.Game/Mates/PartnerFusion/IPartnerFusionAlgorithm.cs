using WingsEmu.Game.Configurations;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Mates.PartnerFusion;

public interface IPartnerFusionAlgorithm
{
    PartnerFusionInfo GetPartnerFusionData(byte level, long percentage, int materialLevel, bool doubleExp);
    SpPerfStats GetPartnerRandomStat(GameItemInstance partnerPsp);
}