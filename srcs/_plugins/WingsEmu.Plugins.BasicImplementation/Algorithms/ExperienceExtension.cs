using System;
using System.Collections.Generic;
using System.Linq;
using NosEmu.Plugins.BasicImplementations.Event.Algorithm;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public static class ExperienceExtension
{
    public static ExcessExperience GetMoreExperience(this IPlayerEntity character, IServerManager serverManager, ILandOfDeathManager landOfDeathManager, LandOfDeathConfiguration landOfDeathConfiguration, IEvtbConfiguration evtbConfiguration)
    {
        double lowLevel = 1;
        double lowJob = 1;
        double lowJobSp = 1;
        double mates = 1;

        lowLevel = character.Level switch
        {
            <= 5 => 3,
            <= 18 => 2,
            _ => lowLevel
        };

        lowJob = character.Level switch
        {
            <= 12 => 3,
            <= 20 => 2,
            _ => lowJob
        };

        double additionalLevel = 1;
        double additionalJob = 1;
        double additionalHeroLevel = 1;
        double additionalMateLevel = 1;
        double additionalPartnerLevel = 1;
        double additionalSp = 0;

        int increaseExperienceBuff = character.BCardComponent.GetAllBCardsInformation(BCardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, character.Level).firstData +
            character.BCardComponent.GetAllBCardsInformation(BCardType.AllExperienceGain, (byte)AdditionalTypes.AllExperienceGain.AllExperienceGainIncrease, character.Level).firstData;
        increaseExperienceBuff += character.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.INCREASE_XP).Item1 ?? 0;
        additionalLevel += increaseExperienceBuff * 0.01;
        additionalLevel += character.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.EXPERIENCE_HERO_BOOK) * 0.01;
        additionalLevel += character.HaveStaticBonus(StaticBonusType.EreniaMedal) ? 0.15 : 0;
        additionalLevel += character.HaveStaticBonus(StaticBonusType.AdventurerMedal) ? 0.15 : 0;
        additionalLevel += character.HaveStaticBonus(StaticBonusType.FriendshipMedal) ? 0.05 : 0;

        additionalJob += increaseExperienceBuff * 0.01;
        additionalJob += character.HaveStaticBonus(StaticBonusType.EreniaMedal) ? 0.15 : 0;
        additionalJob += character.HaveStaticBonus(StaticBonusType.AdventurerMedal) ? 0.15 : 0;
        additionalJob += character.HaveStaticBonus(StaticBonusType.FriendshipMedal) ? 0.05 : 0;

        int increaseHeroExperienceBuff = character.BCardComponent.GetAllBCardsInformation(BCardType.ReputHeroLevel, (byte)AdditionalTypes.ReputHeroLevel.ReceivedHeroExpIncrease, character.Level).firstData;

        increaseHeroExperienceBuff = character.BCardComponent.GetAllBCardsInformation(BCardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, character.Level).firstData +
            character.BCardComponent.GetAllBCardsInformation(BCardType.AllExperienceGain, (byte)AdditionalTypes.AllExperienceGain.AllExperienceGainIncrease, character.Level).firstData;

        increaseHeroExperienceBuff += (character.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.INCREASE_XP).Item1 ?? 0);
        increaseHeroExperienceBuff += (character.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.CHAMPION_XP_BOOST).Item1 ?? 0);

        additionalHeroLevel += increaseHeroExperienceBuff * 0.01;

        additionalMateLevel += character.BCardComponent.GetAllBCardsInformation(BCardType.AllExperienceGain,
            (byte)AdditionalTypes.AllExperienceGain.AllExperienceGainIncrease, character.Level).firstData * 0.01;
        additionalMateLevel += character.HaveStaticBonus(StaticBonusType.EreniaMedal) ? 0.15 : 0;
        additionalMateLevel += character.HaveStaticBonus(StaticBonusType.AdventurerMedal) ? 0.15 : 0;
        additionalMateLevel += character.HaveStaticBonus(StaticBonusType.FriendshipMedal) ? 0.05 : 0;
        additionalPartnerLevel += character.BCardComponent.GetAllBCardsInformation(BCardType.AllExperienceGain,
            (byte)AdditionalTypes.AllExperienceGain.AllExperienceGainIncrease, character.Level).firstData * 0.01;
        additionalPartnerLevel += character.HaveStaticBonus(StaticBonusType.EreniaMedal) ? 0.15 : 0;
        additionalPartnerLevel += character.HaveStaticBonus(StaticBonusType.AdventurerMedal) ? 0.15 : 0;
        additionalPartnerLevel += character.HaveStaticBonus(StaticBonusType.FriendshipMedal) ? 0.05 : 0;

        if (character.HasBuff(BuffVnums.GUARDIAN_BLESS))
        {
            additionalMateLevel += 0.5;
            additionalPartnerLevel += 0.5;
        }

        if (character.HasBuff(BuffVnums.SOULSTONE_BLESSING))
        {
            additionalSp += 0.5;
        }

        IReadOnlyCollection<FamilyBuffCrossChannel> familyBuffs = StaticFamilyManager.Instance.CurrentFamilyBuffs;
        FamilyBuffCrossChannel familyBuff = familyBuffs.FirstOrDefault(x => x?.BuffVnum == (int)BuffVnums.FAMILY_BUFF_XP);
        if (familyBuff != null)
        {
            additionalLevel += character.Family?.Id == familyBuff.FamilyId ? 0.2 : 0.1;
        }

        additionalLevel += character.GetMaxWeaponShellValue(ShellEffectType.GainMoreXP, true) * 0.01;
        additionalJob += character.GetMaxWeaponShellValue(ShellEffectType.GainMoreCXP, true) * 0.01;
        double additionalJobSp = additionalJob + additionalSp;

        IMateEntity mate = character.MateComponent.GetMate(x => x.MateType == MateType.Pet && x.IsTeamMember);
        IMateEntity partner = character.MateComponent.GetMate(x => x.MateType == MateType.Partner && x.IsTeamMember);

        if (mate != null && partner == null)
        {
            mates = mate.IsAlive() ? 1.045 : 0.95;
        }

        if (mate == null && partner != null)
        {
            mates = partner.IsAlive() ? 1.0625 : 0.85;
        }

        if (mate != null && partner != null)
        {
            if (mate.IsAlive() && partner.IsAlive())
            {
                mates = 1.08;
            }
            else if (mate.IsAlive() && !partner.IsAlive())
            {
                mates = 0.88;
            }
            else if (!mate.IsAlive() && partner.IsAlive())
            {
                mates = 1;
            }
            else
            {
                mates = 0.8;
            }
        }
        
        if (character is { SubClass: SubClassType.OathKeeper })
        {
            double tierBonus = character.TierLevel switch
            {
                1 => 0.10, // Add 10% for Tier Level 1
                2 => 0.11, // Add 11% for Tier Level 2
                3 => 0.12, // Add 12% for Tier Level 3
                4 => 0.13, // Add 13% for Tier Level 4
                5 => 0.14, // Add 14% for Tier Level 5
                _ => 0.0  // Default to 0% if no tier level specified
            };
            
            additionalLevel += tierBonus;
            additionalJob += tierBonus;
            additionalMateLevel += tierBonus;
            additionalPartnerLevel += tierBonus;
        }
        
        if (character is { SubClass: SubClassType.ShadowHunter })
        {
            double tierBonus = character.TierLevel switch
            {
                1 => 0.10, // Add 10% for Tier Level 1
                2 => 0.11, // Add 11% for Tier Level 2
                3 => 0.12, // Add 12% for Tier Level 3
                4 => 0.13, // Add 13% for Tier Level 4
                5 => 0.14, // Add 14% for Tier Level 5
                _ => 0.10
            };
            
            additionalHeroLevel += tierBonus;
        }

        int eventIncreaseExperience = evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_EXPERIENCE_EARNED);
        
        double multiplier = eventIncreaseExperience >= 1 ? eventIncreaseExperience : 1;
        
        additionalLevel *= serverManager.MobXpRate * multiplier;
        additionalHeroLevel *= serverManager.HeroXpRate * 0.1 * multiplier;
        additionalJob *= serverManager.JobXpRate * multiplier;
        additionalJobSp *= serverManager.JobXpRate * multiplier;
        additionalMateLevel *= serverManager.MateXpRate * multiplier;
        additionalPartnerLevel *= serverManager.PartnerXpRate * multiplier;

        if (character.Specialist != null && character.UseSp) 
        {
            additionalJob = character.Specialist.SpLevel < 20 ? 0 : additionalJobSp / 2.0;

            lowJobSp = character.Specialist.SpLevel switch
            {
                <= 9 => 10,
                <= 17 => 5,
                _ => lowJobSp
            };
        }
        else
        {
            additionalJobSp = 0;
        }

        double landOfDeathExp = 1;
        if (character.MapInstance is { MapInstanceType: MapInstanceType.LandOfDeath } && landOfDeathManager.IsActive && landOfDeathManager.IsDevilActive)
        {
            landOfDeathExp = landOfDeathConfiguration.XpMultiplier;
        }

        additionalLevel *= landOfDeathExp;
        additionalJob *= landOfDeathExp;
        additionalJobSp *= landOfDeathExp;
        additionalMateLevel *= landOfDeathExp;
        additionalPartnerLevel *= landOfDeathExp;

        return new ExcessExperience(additionalLevel, additionalJob, additionalJobSp, additionalHeroLevel, additionalMateLevel, additionalPartnerLevel, lowLevel, lowJob, lowJobSp, mates);
    }
}