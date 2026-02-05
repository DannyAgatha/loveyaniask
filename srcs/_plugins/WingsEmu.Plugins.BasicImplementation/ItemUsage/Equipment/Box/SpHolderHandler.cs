using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Equipment.Box;

public class SpHolderHandler : IItemUsageByVnumHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _languageService;

    public SpHolderHandler(IGameLanguageService languageService, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _languageService = languageService;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public long[] Vnums => new[] { (long)ItemVnums.SPECIALIST_CARD_HOLDER, (long)ItemVnums.GOLDEN_SP_HOLDER, (long)ItemVnums.PSP_HOLDER };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        int holderVnum = e.Item.ItemInstance.ItemVNum;
        InventoryItem box = session.PlayerEntity.GetItemBySlotAndType(e.Item.Slot, InventoryType.Equipment);

        if (box == null)
        {
            return;
        }

        if (box.ItemInstance.Type != ItemInstanceType.BoxInstance)
        {
            return;
        }

        GameItemInstance boxItem = box.ItemInstance;

        if (boxItem.HoldingVNum is null or 0)
        {
            session.SendWopenPacket((byte)WindowType.GOLDEN_SP_CARD_HOLDER, e.Item.Slot, holderVnum == (int)ItemVnums.PSP_HOLDER ? 1 : 0);
            return;
        }

        if (!session.PlayerEntity.HasSpaceFor(boxItem.HoldingVNum.Value))
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        GameItemInstance newItem = _gameItemInstanceFactory.CreateSpecialistCard(boxItem.HoldingVNum.Value, boxItem.SpLevel, boxItem.Upgrade);

        if (newItem.Type != ItemInstanceType.SpecialistInstance)
        {
            await session.AddNewItemToInventory(newItem);
            return;
        }

        GameItemInstance sp = newItem;

        if (holderVnum == (int)ItemVnums.PSP_HOLDER)
        {
            sp.SkillRank1 = boxItem.SkillRank1;
            sp.SkillRank2 = boxItem.SkillRank2;
            sp.SkillRank3 = boxItem.SkillRank3;
            sp.PartnerSkill1 = boxItem.PartnerSkill1;
            sp.PartnerSkill2 = boxItem.PartnerSkill2;
            sp.PartnerSkill3 = boxItem.PartnerSkill3;
            sp.PartnerSkills = boxItem.PartnerSkills;
            sp.SlDamage = boxItem.SlDamage;
            sp.SlDefence = boxItem.SlDefence;
            sp.SpCriticalDefense = boxItem.SpCriticalDefense;
            sp.SlHp = boxItem.SlHp;
            sp.SpDamage = boxItem.SpDamage;
            sp.SpDark = boxItem.SpDark;
            sp.SpDefence = boxItem.SpDefence;
            sp.SpElement = boxItem.SpElement;
            sp.SpFire = boxItem.SpFire;
            sp.SpHP = boxItem.SpHP;
            sp.SpLight = boxItem.SpLight;
            sp.SpStoneUpgrade = boxItem.SpStoneUpgrade;
            sp.SpWater = boxItem.SpWater;
            sp.Xp = boxItem.Xp;
        }
        else
        {
            sp.SlDamage = boxItem.SlDamage;
            sp.SlDefence = boxItem.SlDefence;
            sp.SlElement = boxItem.SlElement;
            sp.SlHp = boxItem.SlHp;
            sp.SpDamage = boxItem.SpDamage;
            sp.SpDark = boxItem.SpDark;
            sp.SpDefence = boxItem.SpDefence;
            sp.SpElement = boxItem.SpElement;
            sp.SpFire = boxItem.SpFire;
            sp.SpHP = boxItem.SpHP;
            sp.SpLevel = boxItem.SpLevel;
            sp.SpLight = boxItem.SpLight;
            sp.SpStoneUpgrade = boxItem.SpStoneUpgrade;
            sp.SpWater = boxItem.SpWater;
            sp.Upgrade = boxItem.Upgrade;
            sp.Xp = boxItem.Xp;
            sp.IsSecondSpecialistSlotActivated = false;
            sp.IsThirdSpecialistSlotActivated = false;
            sp.Skin = 0;
        }
        
        await session.AddNewItemToInventory(sp);
        await session.RemoveItemFromInventory(item: box);
    }
}