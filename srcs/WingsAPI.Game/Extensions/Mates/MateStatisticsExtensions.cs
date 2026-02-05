using System;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Extensions.Mates;

public static class MateStatisticsExtensions
{
    public static int HealthHpLoad(this IMateEntity mateEntity)
    {
        if (mateEntity.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            return 0;
        }
        
        int regen = mateEntity.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryIncreased, mateEntity.Level).firstData
            - mateEntity.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryDecreased, mateEntity.Level).firstData;
        return mateEntity.IsSitting ? regen + 50 : (DateTime.UtcNow - mateEntity.LastDefence).TotalSeconds > 4 ? regen + 20 : 0;
    }

    public static int HealthMpLoad(this IMateEntity mateEntity)
    {
        if (mateEntity.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            return 0;
        }
        
        int regen = mateEntity.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryIncreased, mateEntity.Level).firstData
            - mateEntity.BCardComponent.GetAllBCardsInformation(BCardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryDecreased, mateEntity.Level).firstData;
        return mateEntity.IsSitting ? regen + 50 :
            (DateTime.UtcNow - mateEntity.LastDefence).TotalSeconds > 4 ? regen + 20 : 0;
    }
}