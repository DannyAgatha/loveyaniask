using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Extensions;

public static class SpecialistExtension
{
    public static string GenerateSkillInfo(this GameItemInstance specialistInstance, byte type)
    {
        // type = 0 - in packet
        // type = 1 - pski packet
        // type = 2 - sc_n packet

        if (specialistInstance == null && type == 2)
        {
            return "-1 -1 -1";
        }

        if (specialistInstance?.PartnerSkills == null)
        {
            return type switch
            {
                0 => "0 0 0 ",
                1 => string.Empty,
                2 => "0.0 0.0 0.0",
                _ => string.Empty
            };
        }

        string generatePacket = string.Empty;
        var generateSkillsPacket = Enumerable.Repeat<PartnerSkill>(null, 3).ToList();

        for (int i = 0; i < 3; i++)
        {
            PartnerSkill skill = specialistInstance.PartnerSkills?.ElementAtOrDefault(i);
            if (skill == null)
            {
                continue;
            }

            generateSkillsPacket[skill.Slot] = skill;
        }

        for (int i = 0; i < 3; i++)
        {
            PartnerSkill skill = generateSkillsPacket.ElementAtOrDefault(i);
            if (skill == null)
            {
                generatePacket += type switch
                {
                    0 => "0 ",
                    1 => string.Empty,
                    2 => "0.0 ",
                    _ => string.Empty
                };

                continue;
            }

            generatePacket += type switch
            {
                0 => $"{skill.SkillId} ",
                1 => $"{skill.SkillId} ",
                2 => $"{skill.SkillId}.{skill.Rank} ",
                _ => "0 0 0"
            };
        }

        return generatePacket;
    }

    public static bool IsSpSkill(this GameItemInstance spInstance, SkillDTO ski) =>
        ski.UpgradeType == spInstance.GameItem.Morph && ski.SkillType == SkillType.NormalPlayerSkill && spInstance.SpLevel >= ski.LevelMinimum;
    
    public static void AssignSubClassSkill(this IClientSession session, SubClassType subClass, int tierLevel)
    {
        var baseSkillVnums = new Dictionary<SubClassType, int>
        {
            { SubClassType.OathKeeper, (int)SkillsVnums.OATH_KEEPER_T1_SKILL },
            { SubClassType.CrimsonFury, (int)SkillsVnums.CRIMSON_FURY_T1_SKILL },
            { SubClassType.CelestialPaladin, (int)SkillsVnums.CELESTIAL_PALADIN_T1_SKILL },
            { SubClassType.SilentStalker, (int)SkillsVnums.SILENT_STALKER_T1_SKILL },
            { SubClassType.ArrowLord, (int)SkillsVnums.ARROW_LORD_T1_SKILL },
            { SubClassType.ShadowHunter, (int)SkillsVnums.SHADOW_HUNTER_T1_SKILL },
            { SubClassType.ArcaneSage, (int)SkillsVnums.ARCANE_SAGE_T1_SKILL },
            { SubClassType.Pyromancer, (int)SkillsVnums.PYROMANCER_T1_SKILL },
            { SubClassType.DarkNecromancer, (int)SkillsVnums.DARK_NECROMANCER_T1_SKILL },
            { SubClassType.ZenWarrior, (int)SkillsVnums.ZEN_WARRIOR_T1_SKILL },
            { SubClassType.EmperorsBlade, (int)SkillsVnums.EMPERORS_BLADE_T1_SKILL },
            { SubClassType.StealthShadow, (int)SkillsVnums.STEALTH_SHADOW_T1_SKILL }
        };
        
        if (baseSkillVnums.TryGetValue(session.PlayerEntity.SubClass, out int oldBaseVnum))
        {
            for (int tier = 1; tier <= 5; tier++)
            {
                int skillToRemove = oldBaseVnum + (tier - 1);
                if (session.PlayerEntity.UseSp)
                {
                    if (session.PlayerEntity.SkillsSp.ContainsKey((short)skillToRemove))
                    {
                        session.PlayerEntity.SkillsSp.Remove((short)skillToRemove, out CharacterSkill _);
                    }
                }
                
                session.PlayerEntity.CharacterSkills.TryRemove(skillToRemove, out _);
            }
        }
        
        if (!baseSkillVnums.TryGetValue(subClass, out int newBaseVnum))
        {
            return;
        }
        
        int newSkillId = newBaseVnum + (tierLevel - 1);
        var newSkill = new CharacterSkill { SkillVNum = newSkillId };
        if (session.PlayerEntity.UseSp)
        {
            session.PlayerEntity.SkillsSp[(short)newSkillId] = newSkill;
            session.PlayerEntity.Skills.Add(newSkill);
        }
        session.PlayerEntity.CharacterSkills.TryAdd(newSkillId, newSkill);
    }

    public static void SendPartnerSpecialistInfo(this IClientSession session, GameItemInstance item) =>
        session.SendPacket(item.GeneratePslInfo());

    public static string GeneratePslInfo(this GameItemInstance item) =>
         "pslinfo " +
         $"{item.ItemVNum} " +
         $"{item.GameItem.Element} " +
         $"{item.GameItem.ElementRate} " +
         $"{item.GameItem.LevelMinimum} " +
         $"{item.GameItem.Speed} " +
         $"{item.GameItem.FireResistance} " +
         $"{item.GameItem.WaterResistance} " +
         $"{item.GameItem.LightResistance} " +
         $"{item.GameItem.DarkResistance} " +
         $"{item.GenerateSkillInfo(2)} " +
         $"{item.SpStoneUpgrade} " +
         $"{item.SpDamage} " +
         $"{item.SpDefence} " +
         $"{item.SpCriticalDefense} " +
         $"{item.SpHP} " +
         $"{item.SpFire} " +
         $"{item.SpWater} " +
         $"{item.SpLight} " +
         $"{item.SpDark}";
}