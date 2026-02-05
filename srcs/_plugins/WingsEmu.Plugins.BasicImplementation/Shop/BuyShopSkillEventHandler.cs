using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Shop;

public class BuyShopSkillEventHandler : IAsyncEventProcessor<BuyShopSkillEvent>
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISkillsManager _skillsManager;

    public BuyShopSkillEventHandler(IGameLanguageService gameLanguage, ISkillsManager skillsManager, ICharacterAlgorithm characterAlgorithm)
    {
        _gameLanguage = gameLanguage;
        _skillsManager = skillsManager;
        _characterAlgorithm = characterAlgorithm;
    }

    public async Task HandleAsync(BuyShopSkillEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        short newSkillVnum = e.Slot;
        long ownerId = e.OwnerId;
        bool accept = e.Accept;

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(ownerId);

        if (npcEntity.ShopNpc.ShopSkills.All(s => s.SkillVNum != newSkillVnum))
        {
            return;
        }

        // skill shop
        if (session.PlayerEntity.UseSp)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_REMOVE_SP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.SKILL_CHATMESSAGE_CANT_LEARN_MORPHED), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.PlayerEntity.Skills.Any(s => !session.PlayerEntity.SkillCanBeUsed(s)))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CANT_LEARN_COOLDOWN), MsgMessageType.Middle);
            return;
        }

        SkillDTO skillInfo = _skillsManager.GetSkill(newSkillVnum);

        if (skillInfo == null)
        {
            return;
        }

        if (session.PlayerEntity.CharacterSkills.Any(s => s.Value.SkillVNum == newSkillVnum))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.SKILL_CHATMESSAGE_ALREADY_LEARNT), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.Gold < skillInfo.Price)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.GetCp() < skillInfo.CPCost && !skillInfo.IsPassiveSkill())
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NOT_ENOUGH_CP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (skillInfo.IsPassiveSkill())
        {
            int skillMinimumLevel = 0;
            if (skillInfo.MinimumSwordmanLevel == 0 && skillInfo.MinimumArcherLevel == 0 && skillInfo.MinimumMagicianLevel == 0)
            {
                skillMinimumLevel = skillInfo.MinimumAdventurerLevel;
            }
            else
            {
                skillMinimumLevel = session.PlayerEntity.Class switch
                {
                    ClassType.Adventurer => skillInfo.MinimumAdventurerLevel,
                    ClassType.Swordman => skillInfo.MinimumSwordmanLevel,
                    ClassType.Archer => skillInfo.MinimumArcherLevel,
                    ClassType.Magician => skillInfo.MinimumMagicianLevel,
                    _ => skillMinimumLevel
                };
            }

            if (skillMinimumLevel == 0)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CANT_LEARN, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.Level < skillMinimumLevel)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_TOO_LOW_LVL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            // Find higher passive already in PlayerEntity
            CharacterSkill findHigherPassive = session.PlayerEntity.CharacterSkills.Values.FirstOrDefault(x => x.Skill.IsPassiveSkill()
                && x.Skill.CastId == skillInfo.CastId && x.Skill.Id > skillInfo.Id);

            if (findHigherPassive != null)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CANT_LEARN, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            foreach (CharacterSkill skill in session.PlayerEntity.CharacterSkills.Values)
            {
                if (skillInfo.CastId == skill.Skill.CastId && skill.Skill.IsPassiveSkill())
                {
                    session.PlayerEntity.CharacterSkills.TryRemove(skill.SkillVNum, out CharacterSkill value);
                    session.PlayerEntity.Skills.Remove(value);
                }
            }
        }
        else
        {
            if ((byte)session.PlayerEntity.Class != skillInfo.Class)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CANT_LEARN, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.JobLevel < skillInfo.LevelMinimum)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_LOW_JOB, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (skillInfo.UpgradeSkill != 0)
            {
                if (!session.PlayerEntity.CharacterSkills.ContainsKey(skillInfo.UpgradeSkill))
                {
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.SKILL_CHATMESSAGE_CANT_LEARN_NEED_BASE), ChatMessageColorType.Yellow);
                    return;
                }

                CharacterSkill oldUpgrade = session.PlayerEntity.CharacterSkills.Values.FirstOrDefault(s =>
                    s.Skill.UpgradeSkill == skillInfo.UpgradeSkill && s.Skill.UpgradeType == skillInfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                if (oldUpgrade != null)
                {
                    if (!accept)
                    {
                        session.SendQnaPacket($"buy 2 {npcEntity.Id} {newSkillVnum} 1", session.GetLanguage(GameDialogKey.SKILL_DIALOG_CONFIRM_REPLACE_UPGRADE));
                        return;
                    }

                    session.PlayerEntity.CharacterSkills.TryRemove(oldUpgrade.SkillVNum, out CharacterSkill value);
                    session.PlayerEntity.Skills.Remove(value);
                    if (session.PlayerEntity.SkillComponent.SkillUpgrades.TryGetValue(skillInfo.UpgradeSkill, out HashSet<IBattleEntitySkill> hashSet))
                    {
                        hashSet.Remove(value);
                    }
                }
            }
        }

        var newSkill = new CharacterSkill { SkillVNum = newSkillVnum };

        short upgradeSkill = newSkill.Skill.UpgradeSkill;
        if (upgradeSkill != 0)
        {
            if (!session.PlayerEntity.SkillComponent.SkillUpgrades.TryGetValue(upgradeSkill, out HashSet<IBattleEntitySkill> hashSet))
            {
                hashSet = new HashSet<IBattleEntitySkill>();
                session.PlayerEntity.SkillComponent.SkillUpgrades[upgradeSkill] = hashSet;
            }

            hashSet.Add(newSkill);
        }

        session.PlayerEntity.CharacterSkills[newSkillVnum] = newSkill;
        session.PlayerEntity.Skills.Add(newSkill);
        session.PlayerEntity.Gold -= skillInfo.Price;
        session.RefreshGold();
        session.RefreshPassiveBCards();
        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_LEARNED, session.UserLanguage), MsgMessageType.Middle);
        session.RefreshLevel(_characterAlgorithm);
        session.SendSound(SoundType.BUY_SKILL);
        await session.EmitEventAsync(new ShopSkillBoughtEvent
        {
            SkillVnum = newSkillVnum
        });
    }
}