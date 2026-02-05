using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Npc;

public class SellPacketHandler : GenericGamePacketHandlerBase<SellPacket>
{
    private static readonly HashSet<short> Basics = new() { 200, 201, 220, 221, 240, 241, 260, 261, 1525, 1529 };
    private static readonly HashSet<short> Capture = new() { 209, 235, 236, 237, 1565 };
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _language;
    private readonly IServerManager _serverManager;

    public SellPacketHandler(IServerManager serverManager, ICharacterAlgorithm characterAlgorithm, IGameLanguageService language)
    {
        _characterAlgorithm = characterAlgorithm;
        _serverManager = serverManager;
        _language = language;
    }

    protected override async Task HandlePacketAsync(IClientSession session, SellPacket sellPacket)
    {
        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.IsShopping)
        {
            return;
        }

        if (sellPacket.Amount.HasValue && sellPacket.Slot.HasValue)
        {
            await HandleItemPacket(session, sellPacket);
            return;
        }

        HandleSkillPacket(session, sellPacket);
    }

    private void HandleSkillPacket(IClientSession session, SellPacket sellPacket)
    {
        if (session.PlayerEntity.UseSp)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_REMOVE_SP), MsgMessageType.Middle);
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

        if (session.PlayerEntity.Skills.Any(s => !session.PlayerEntity.SkillCanBeUsed(s, DateTime.UtcNow)))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CANT_LEARN_COOLDOWN), MsgMessageType.Middle);
            return;
        }

        short vnum = sellPacket.Data;

        if (!session.PlayerEntity.CharacterSkills.TryGetValue(vnum, out CharacterSkill skill))
        {
            return;
        }

        if (skill == null || IsBasicSkillOrCapture(vnum))
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.SKILL_CHATMESSAGE_NOT_TO_REFUND, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (skill.Skill.IsPassiveSkill())
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.SKILL_CHATMESSAGE_NOT_TO_REFUND, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (skill.Skill.UpgradeSkill != 0)
        {
            if (session.PlayerEntity.SkillComponent.SkillUpgrades.TryGetValue(skill.Skill.UpgradeSkill, out HashSet<IBattleEntitySkill> hashSet))
            {
                hashSet.Remove(skill);
            }
        }

        foreach (CharacterSkill loadedSkill in session.PlayerEntity.CharacterSkills.Values)
        {
            if (skill.SkillVNum != loadedSkill.Skill.UpgradeSkill)
            {
                continue;
            }

            session.PlayerEntity.CharacterSkills.TryRemove(loadedSkill.SkillVNum, out CharacterSkill value);
            session.PlayerEntity.Skills.Remove(value);
        }

        session.PlayerEntity.CharacterSkills.TryRemove(skill.SkillVNum, out CharacterSkill _);
        session.PlayerEntity.Skills.Remove(skill);

        if (session.PlayerEntity.SkillComponent.SkillUpgrades.TryGetValue(vnum, out HashSet<IBattleEntitySkill> upgrades))
        {
            upgrades.Clear();
        }

        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.RefreshLevel(_characterAlgorithm);

        session.EmitEventAsync(new ShopSkillSoldEvent
        {
            SkillVnum = skill.SkillVNum
        });
    }

    private async Task HandleItemPacket(IClientSession session, SellPacket sellPacket)
    {
        var type = (InventoryType)sellPacket.Data;
        byte slot = sellPacket.Slot ?? 0;
        ushort amount = sellPacket.Amount ?? 1;

        InventoryItem inv = session.PlayerEntity.GetItemBySlotAndType(slot, type);
        if (inv == null)
        {
            return;
        }

        if (amount > inv.ItemInstance.Amount)
        {
            return;
        }

        if (inv.ItemInstance.GameItem.Type == InventoryType.Miniland &&
            session.PlayerEntity.Miniland != null && session.PlayerEntity.Miniland.MapDesignObjects.Any(s => s.InventorySlot == inv.Slot))
        {
            return;
        }

        if (inv.ItemInstance.GameItem.ReputPrice != 0)
        {
            return;
        }

        if (!inv.ItemInstance.GameItem.IsSoldable)
        {
            session.SendSMemo(SmemoType.Error, _language.GetLanguage(GameDialogKey.INTERACTION_LOG_ITEM_NOT_SELLABLE, session.UserLanguage));
            return;
        }

        long price = inv.ItemInstance.GameItem.ItemType == ItemType.Sell ? inv.ItemInstance.GameItem.Price : inv.ItemInstance.GameItem.Price / 20 <= 0 ? 1 : inv.ItemInstance.GameItem.Price / 20;

        if (session.PlayerEntity.Gold + price * amount > _serverManager.MaxGold)
        {
            session.SendSMemo(SmemoType.Error, _language.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_MAX_GOLD, session.UserLanguage));
            return;
        }

        session.PlayerEntity.Gold += price * amount;
        session.RefreshGold();
        session.SendSMemo(SmemoType.Balance, _language.GetLanguage(GameDialogKey.SHOP_LOG_SELL_ITEM_VALID, session.UserLanguage));
        await session.RemoveItemFromInventory(item: inv, amount: (short)amount);
        await session.EmitEventAsync(new ShopNpcSoldItemEvent
        {
            ItemInstance = inv.ItemInstance,
            Amount = (short)amount,
            PricePerItem = price
        });
    }

    private static bool IsBasicSkillOrCapture(short vnum) => Basics.Contains(vnum) || Capture.Contains(vnum);
}