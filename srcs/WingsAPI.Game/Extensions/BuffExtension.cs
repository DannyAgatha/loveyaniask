using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Extensions;

public static class BuffExtension
{
    private static readonly HashSet<short> _buffResistance = new()
    {
        (short)BCardType.DebuffResistance,
        (short)BCardType.AngerSkill,
        (short)BCardType.SpecialisationBuffResistance,
        (short)BCardType.DragonSkills,
        (short)BCardType.Buff
    };

    public static bool IsNormal(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.NORMAL);
    public static bool IsNotRemovedOnDeath(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.NOT_REMOVED_ON_DEATH);
    public static bool IsNotDisappearOnSpChange(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.NOT_REMOVED_ON_SP_CHANGE);
    public static bool IsRefreshAtExpiration(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.REFRESH_AT_EXPIRATION);
    public static bool IsDisappearOnPvp(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.DISAPPEAR_ON_PVP);
    public static bool IsBigBuff(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.BIG);
    public static bool IsNoDuration(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.NO_DURATION);
    public static bool IsSavingOnDisconnect(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.SAVING_ON_DISCONNECT);
    public static bool IsPartnerBuff(this Buff buff) => buff != null && buff.BuffFlags.HasFlag(BuffFlag.PARTNER);
    public static bool IsPartnerRankBuff(this int buffVnum) => buffVnum >= 2000;

    public static int RemainingTimeInMilliseconds(this Buff buff) => (int)(buff.Start - DateTime.UtcNow + buff.Duration).TotalMilliseconds;

    public static async Task CheckPartnerBuff(this IClientSession session) => await session.EmitEventAsync(new BuffPartnerCheckEvent());

    public static double CheckForResistance(this IBattleEntity target, Buff b, ICardsManager cardsManager, out double buffCounter, out double specializedResistance)
    {
        var bCards = new List<(int casterLevel, BCardDTO bCard)>();

        IReadOnlyList<BCardDTO> buffDebuffResistance = target.BCardComponent.GetAllBCards();
        IReadOnlyList<(int casterLevel, BCardDTO bCard)> buffsBCards = target.BCardComponent.GetBuffBCards();

        foreach (BCardDTO bCard in buffDebuffResistance)
        {
            bCards.Add((target.Level, bCard));
        }

        bCards.AddRange(buffsBCards);

        double debuffCounter = 1;
        buffCounter = 1;
        specializedResistance = 1;

        int secondDataSum = 0;
        foreach ((int casterLevel, BCardDTO resistanceBCard) in bCards)
        {
            if (!_buffResistance.Contains((short)resistanceBCard.Type))
            {
                continue;
            }

            int firstDataValue = resistanceBCard.FirstDataValue(casterLevel);
            int secondDataValue = resistanceBCard.SecondDataValue(casterLevel);

            switch (resistanceBCard.Type)
            {
                case (short)BCardType.SpecialisationBuffResistance when resistanceBCard.SubType == (byte)AdditionalTypes.SpecialisationBuffResistance.ResistanceToEffect ||
                    resistanceBCard.SubType == (byte)AdditionalTypes.SpecialisationBuffResistance.ResistanceToEffectNegated:
                    {
                        if (secondDataValue == 0)
                        {
                            continue;
                        }

                        Card anotherBuff = cardsManager.GetCardByCardId(secondDataValue);
                        if (anotherBuff == null)
                        {
                            continue;
                        }

                        if (b.GroupId == anotherBuff.GroupId && b.Level <= anotherBuff.Level)
                        {
                            specializedResistance *= 1.0 - firstDataValue / 100.0;
                        }

                        break;
                    }
                case (short)BCardType.DragonSkills when resistanceBCard.SubType == (byte)AdditionalTypes.DragonSkills.CannotUseBuffChance:
                    {
                        if (b.Level <= resistanceBCard.SecondData)
                        {
                            buffCounter *= 1.0 - firstDataValue / 100.0;
                        }

                        break;
                    }
                case (short)BCardType.DebuffResistance when b.BuffGroup == BuffGroup.Bad:
                    {
                        switch (resistanceBCard.SubType)
                        {
                            case (byte)AdditionalTypes.DebuffResistance.NeverBadDiseaseEffectChance when b.BuffCategory == BuffCategory.DiseaseSeries && b.Level <= firstDataValue:
                                secondDataSum -= secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.NeverBadGeneralEffectChance when b.BuffCategory == BuffCategory.GeneralEffect && b.Level <= firstDataValue:
                                secondDataSum -= secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.NeverBadMagicEffectChance when b.BuffCategory == BuffCategory.MagicEffect && b.Level <= firstDataValue:
                                secondDataSum -= secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.NeverBadToxicEffectChance when b.BuffCategory == BuffCategory.PoisonType && b.Level <= firstDataValue:
                                secondDataSum -= secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.IncreaseBadDiseaseEffectChance when b.BuffCategory == BuffCategory.DiseaseSeries && b.Level <= firstDataValue:
                                secondDataSum += secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.IncreaseBadGeneralEffectChance when b.BuffCategory == BuffCategory.GeneralEffect && b.Level <= firstDataValue:
                                secondDataSum += secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.IncreaseBadMagicEffectChance when b.BuffCategory == BuffCategory.MagicEffect && b.Level <= firstDataValue:
                                secondDataSum += secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.IncreaseBadToxicEffectChance when b.BuffCategory == BuffCategory.PoisonType && b.Level <= firstDataValue:
                                secondDataSum += secondDataValue;
                                break;
                        }

                        switch (resistanceBCard.SubType)
                        {
                            case (byte)AdditionalTypes.DebuffResistance.NeverBadEffectChance when b.Level <= firstDataValue:
                                secondDataSum -= secondDataValue;
                                break;
                            case (byte)AdditionalTypes.DebuffResistance.IncreaseBadEffectChance when b.Level <= firstDataValue:
                                secondDataSum += secondDataValue;
                                break;
                        }

                        break;
                    }
                case (short)BCardType.AngerSkill when b.BuffGroup == BuffGroup.Good:
                    switch (resistanceBCard.SubType)
                    {
                        case (byte)AdditionalTypes.AngerSkill.BlockGoodEffectNegated when b.Level <= secondDataValue:
                        case (byte)AdditionalTypes.AngerSkill.BlockGoodEffect when b.Level <= secondDataValue:
                            buffCounter *= 1.0 - firstDataValue / 100.0;
                            break;
                    }

                    break;
                case (short)BCardType.Buff when resistanceBCard.SubType == (byte)AdditionalTypes.Buff.PreventingBadEffect:
                    if (firstDataValue != b.GroupId)
                    {
                        break;
                    }

                    if (b.Level > secondDataValue)
                    {
                        break;
                    }

                    secondDataSum -= 80;

                    break;
            }
        }

        debuffCounter *= 1.0 + secondDataSum / 100.0;

        if (target is not IPlayerEntity playerEntity)
        {
            return debuffCounter;
        }

        int shell = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedAllNegativeEffect);
        int familyBuff = (playerEntity.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.DEBUFF_PROTECTION).Item1 ?? 0);

        debuffCounter *= 1.0 - shell / 100.0;
        debuffCounter *= 1.0 - familyBuff / 100.0;

        int reducedByBuffVnum = 0;

        switch ((BuffVnums)b.CardId)
        {
            case BuffVnums.MINOR_BLEEDING:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedMinorBleeding);
                reducedByBuffVnum += playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedBleedingAndMinorBleeding);
                break;
            case BuffVnums.BLEEDING:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedBleedingAndMinorBleeding);
                break;
            case BuffVnums.BLACKOUT:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedStun);
                break;
            case BuffVnums.HAND_OF_DEATH:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedParalysis);
                break;
            case BuffVnums.FREEZE:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedFreeze);
                break;
            case BuffVnums.BLIND:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedBlind);
                break;
            case BuffVnums.BIND:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedSlow);
                break;
            case BuffVnums.WEAKEN_DEFENCE_POWER:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedArmorDeBuff);
                break;
            case BuffVnums.SHOCK:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedShock);
                break;
            case BuffVnums.PARALYSIS:
                reducedByBuffVnum = playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedPoisonParalysis);
                break;
        }

        debuffCounter *= 1.0 - reducedByBuffVnum / 100.0;

        int reducedByGroupId = (BuffGroupIds)b.GroupId switch
        {
            BuffGroupIds.STUNS => playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedAllStun),
            BuffGroupIds.PARALYSIS => playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedParalysis),
            BuffGroupIds.MANDRA => playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedAllNegativeEffect),
            BuffGroupIds.BLEEDING => playerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedAllBleedingType),
            _ => 0
        };

        debuffCounter *= 1.0 - reducedByGroupId / 100.0;

        return debuffCounter;
    }

    public static void SendBuffsPacket(this IClientSession session)
    {
        if (!session.PlayerEntity.BuffComponent.HasAnyBuff())
        {
            return;
        }

        IReadOnlyList<Buff> buffs = session.PlayerEntity.BuffComponent.GetAllBuffs();
        foreach (Buff buff in buffs)
        {
            if (!buff.IsBigBuff())
            {
                switch (buff.CardId)
                {
                    case (short)BuffVnums.CHARGE when session.PlayerEntity.BCardComponent.GetChargeBCards().Any():
                        int sum = session.PlayerEntity.BCardComponent.GetChargeBCards().Sum(x => x.FirstDataValue(session.PlayerEntity.Level));
                        session.PlayerEntity.SendBfPacket(buff, sum, sum);
                        break;
                    case (short)BuffVnums.CHARGE:
                        session.PlayerEntity.SendBfPacket(buff, session.PlayerEntity.ChargeComponent.GetCharge(), session.PlayerEntity.ChargeComponent.GetCharge());
                        break;
                    default:
                        session.PlayerEntity.SendBfLeftPacket(buff);
                        break;
                }
            }
            else
            {
                session.SendStaticBuffUiPacket(buff, buff.RemainingTimeInMilliseconds());
            }
        }
    }
}