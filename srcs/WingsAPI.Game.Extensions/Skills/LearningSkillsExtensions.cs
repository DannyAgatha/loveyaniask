using System;
using System.Collections.Concurrent;
using System.Linq;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace WingsAPI.Game.Extensions.Quicklist
{
    public static class LearningSkillsExtensions
    {
        public static void LearnAdventurerSkill(this IClientSession session, ISkillsManager skillsManager, IGameLanguageService gameLanguage)
        {
            bool usingSp = session.PlayerEntity.UseSp && session.PlayerEntity.Specialist != null;
            if (session.PlayerEntity.Class != (byte)ClassType.Adventurer)
            {
                if (!usingSp)
                {
                    session.RefreshSkillList();
                    session.RefreshQuicklist();

                    session.PlayerEntity.ClearSkillCooldowns();
                    foreach (IBattleEntitySkill skill in session.PlayerEntity.Skills)
                    {
                        skill.LastUse = DateTime.MinValue;
                    }
                }

                return;
            }

            bool newSkill = false;
            for (int skillVnum = 200; skillVnum <= 210; skillVnum++)
            {
                if (skillVnum == 209)
                {
                    skillVnum++;
                }

                SkillDTO skinfo = skillsManager.GetSkill((short)skillVnum);
                if (skinfo.Class != 0 || session.PlayerEntity.JobLevel < skinfo.LevelMinimum)
                {
                    continue;
                }

                int vnum = skillVnum;
                if (session.PlayerEntity.CharacterSkills.Any(s => s.Value.SkillVNum == vnum))
                {
                    continue;
                }

                newSkill = true;
                var newAdventurerSkill = new CharacterSkill { SkillVNum = (short)skillVnum };
                session.PlayerEntity.CharacterSkills[skillVnum] = newAdventurerSkill;

                if (usingSp)
                {
                    continue;
                }

                session.PlayerEntity.Skills.Add(newAdventurerSkill);
            }

            if (newSkill == false && !usingSp)
            {
                session.RefreshSkillList();
                session.RefreshQuicklist();

                session.PlayerEntity.ClearSkillCooldowns();
                foreach (IBattleEntitySkill skill in session.PlayerEntity.Skills)
                {
                    skill.LastUse = DateTime.MinValue;
                }

                return;
            }

            if (usingSp)
            {
                return;
            }

            session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_LEARNED, session.UserLanguage), MsgMessageType.Middle);
            session.RefreshSkillList();
            session.RefreshQuicklist();

            session.PlayerEntity.ClearSkillCooldowns();
            foreach (IBattleEntitySkill skill in session.PlayerEntity.Skills)
            {
                skill.LastUse = DateTime.MinValue;
            }
        }

        public static void LearnSpSkill(this IClientSession session, ISkillsManager skillsManager, IGameLanguageService gameLanguage)
        {
            byte skillSpCount = (byte)session.PlayerEntity.SkillsSp.Count;
            session.PlayerEntity.SkillsSp = new ConcurrentDictionary<int, CharacterSkill>();

            foreach (SkillDTO ski in skillsManager.GetSkills())
            {
                if (!session.PlayerEntity.Specialist.IsSpSkill(ski))
                {
                    continue;
                }

                var newSkill = new CharacterSkill { SkillVNum = ski.Id };

                session.PlayerEntity.SkillsSp[ski.Id] = newSkill;
                session.PlayerEntity.Skills.Add(newSkill);
            }
            
            session.AssignSubClassSkill(session.PlayerEntity.SubClass, session.PlayerEntity.TierLevel);

            session.PlayerEntity.ClearSkillCooldowns();
            foreach (IBattleEntitySkill skill in session.PlayerEntity.Skills)
            {
                skill.LastUse = DateTime.MinValue;
            }

            session.RefreshSkillList();
            session.RefreshQuicklist();

            if (session.PlayerEntity.SkillsSp.Count == skillSpCount)
            {
                return;
            }

            session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_LEARNED, session.UserLanguage), MsgMessageType.Middle);
        }
    }
}