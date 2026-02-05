using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class MateRevivalConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public List<int> MateInstantRevivalPenalizationSaver { get; set; } = new()
    {
        (int)ItemVnums.POWERFUL_NOSMATE_GUARDIAN_ANGEL,
        (int)ItemVnums.NOSMATE_GUARDIAN_ANGEL_LIMITED,
        (int)ItemVnums.NOSMATE_GUARDIAN_ANGEL
    };

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MateInstantRevivalPenalizationSaverAmount { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public List<int> PartnerInstantRevivalPenalizationSaver { get; set; } = new()
    {
        (int)ItemVnums.POWERFUL_PARTNER_GUARDIAN_ANGEL,
        (int)ItemVnums.PARTNER_GUARDIAN_ANGEL_LIMITED,
        (int)ItemVnums.PARTNER_GUARDIAN_ANGEL
    };

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int PartnerInstantRevivalPenalizationSaverAmount { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public TimeSpan DelayedRevivalDelay { get; set; } = TimeSpan.FromMinutes(3);

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int DelayedRevivalPenalizationSaver { get; set; } = (int)ItemVnums.SEED_OF_POWER;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int DelayedRevivalPenalizationSaverAmount { get; set; } = 5;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short LoyaltyDeathPenalizationAmount { get; set; } = 0;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public AuthorityType NoLoyaltyDeathPenalizationMinAuthority { get; set; } = AuthorityType.VIP_E;
}