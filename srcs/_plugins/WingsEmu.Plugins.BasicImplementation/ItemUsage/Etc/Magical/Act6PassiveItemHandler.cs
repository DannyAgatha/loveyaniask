using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class Act6PassiveItemHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISkillsManager _skillsManager;

    public Act6PassiveItemHandler(IGameLanguageService gameLanguage, ISkillsManager skillsManager)
    {
        _gameLanguage = gameLanguage;
        _skillsManager = skillsManager;
    }

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => [99];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem item = e.Item;
        byte heroLevel = item.ItemInstance.GameItem.LevelMinimum;

        if (session.PlayerEntity.HeroLevel < heroLevel)
        {
            session.SendErrorChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_REQUIERED_LEVEL, session.UserLanguage));
            return;
        }

        SkillDTO skill = _skillsManager.GetSkill(item.ItemInstance.GameItem.EffectValue);

        if (skill == null)
        {
            return;
        }

        if (!skill.IsPassiveSkill())
        {
            return;
        }

        if (session.PlayerEntity.CharacterSkills.ContainsKey(item.ItemInstance.GameItem.EffectValue))
        {
            return;
        }

        CharacterSkill passive = session.PlayerEntity.CharacterSkills.Values.FirstOrDefault(x => x.Skill.CastId == skill.CastId);
        
        if (passive == null && skill.UpgradeSkill != 1)
        {
            return;
        }

        if (passive != null)
        {
            if ((passive.Skill.UpgradeSkill + 1) != skill.UpgradeSkill)
            {
                return;
            }
        }

        foreach (CharacterSkill ski in session.PlayerEntity.CharacterSkills.Values)
        {
            if (skill.CastId == ski.Skill.CastId && ski.Skill.IsPassiveSkill())
            {
                session.PlayerEntity.CharacterSkills.TryRemove(ski.SkillVNum, out CharacterSkill _);
            }
        }

        session.PlayerEntity.CharacterSkills[skill.CastId] = new CharacterSkill
        {
            SkillVNum = item.ItemInstance.GameItem.EffectValue
        };

        session.RefreshPassiveBCards();
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_LEARNED, session.UserLanguage), MsgMessageType.Middle);
        session.RefreshSkillList();
        session.RefreshQuicklist();
        await session.RemoveItemFromInventory(item.ItemInstance.ItemVNum);
    }
}