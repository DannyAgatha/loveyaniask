using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class CostumeFusionEventHandler : IAsyncEventProcessor<CostumeFusionEvent>
{
    public async Task HandleAsync(CostumeFusionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        
        bool hasCostumeFusionItem = session.PlayerEntity.HasItem((short)ItemVnums.COSTUME_FUSION);
        bool hasCostumeFusionItemLimited = session.PlayerEntity.HasItem((short)ItemVnums.COSTUME_FUSION_LIMITED);
        if (!hasCostumeFusionItem && !hasCostumeFusionItemLimited)
        {
            return;
        }
        
        InventoryItem effectsItem = e.LeftItem;
        InventoryItem designItem = e.RightItem;

        GameItemInstance effectsItemInstance = effectsItem.ItemInstance;
        GameItemInstance designItemInstance = designItem.ItemInstance;
        
        if (effectsItemInstance.HasBeenFused || designItemInstance.HasBeenFused)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CannotUseItemWithChangedAppearance);
            session.SendSayi(ChatMessageColorType.Green, Game18NConstString.CannotUseItemWithChangedAppearance);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ErrorCreatingMaterials);
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        if (effectsItemInstance.GameItem.ItemType != ItemType.Fashion || designItemInstance.GameItem.ItemType != ItemType.Fashion)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.COSTUME_FUSION_MESSAGE_NOT_COSTUME), ChatMessageColorType.Red);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }
        
        if ((designItemInstance.GameItem.EquipmentSlot != EquipmentType.CostumeHat || effectsItemInstance.GameItem.EquipmentSlot != EquipmentType.CostumeHat)
            && (effectsItemInstance.GameItem.EquipmentSlot != EquipmentType.CostumeSuit || designItemInstance.GameItem.EquipmentSlot != EquipmentType.CostumeSuit))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.COSTUME_FUSION_MESSAGE_NOT_SAME_ITEM_TYPE), ChatMessageColorType.Red);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        if (effectsItemInstance.GameItem.EquipmentSlot != designItemInstance.GameItem.EquipmentSlot)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.COSTUME_FUSION_MESSAGE_NOT_SAME_ITEM_TYPE), ChatMessageColorType.Red);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        if (effectsItemInstance.ItemDeleteTime is not null || designItemInstance.ItemDeleteTime is not null)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.BothMaterialsMustBePermanent);
            session.SendSayi(ChatMessageColorType.Green, Game18NConstString.BothMaterialsMustBePermanent);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ErrorCreatingMaterials);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        GenderType effectsItemSex = effectsItemInstance.GameItem.Sex switch
        {
            0 => GenderType.Unisex,
            1 => GenderType.Male,
            _ => GenderType.Female
        };
        
        GenderType designItemSex = designItemInstance.GameItem.Sex switch
        {
            0 => GenderType.Unisex,
            1 => GenderType.Male,
            _ => GenderType.Female
        };

        if (effectsItemSex != designItemSex)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CantBeUsed);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ErrorCreatingMaterials);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        if (effectsItemInstance.GameItem.IsLimited != designItemInstance.GameItem.IsLimited)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.BothMaterialsMustBeBound);
            session.SendSayi(ChatMessageColorType.Green, Game18NConstString.BothMaterialsMustBeBound);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ErrorCreatingMaterials);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        if (!effectsItemInstance.BoundCharacterId.HasValue || !designItemInstance.BoundCharacterId.HasValue 
            || effectsItemInstance.BoundCharacterId.Value != designItemInstance.BoundCharacterId.Value)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.BothMaterialsMustBeBound);
            session.SendSayi(ChatMessageColorType.Green, Game18NConstString.BothMaterialsMustBeBound);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ErrorCreatingMaterials);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        if (!session.HasEnoughGold(1_000_000))
        {
            session.SendInfoi(Game18NConstString.NotEnoughFounds);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        session.PlayerEntity.Gold -= 1_000_000;
        session.RefreshGold();
        
        effectsItemInstance.Skin = (short)designItemInstance.GameItem.Id;
        effectsItemInstance.HasBeenFused = true;
        designItemInstance.HasBeenFused = true;
        
        await session.RemoveItemFromInventory(item: designItem);
        
        if (hasCostumeFusionItemLimited)
        {
            await session.RemoveItemFromInventory((short)ItemVnums.COSTUME_FUSION_LIMITED);
        }
        else
        {
            await session.RemoveItemFromInventory((short)ItemVnums.COSTUME_FUSION);
        }
        
        session.SendMsgi(MessageType.Default, Game18NConstString.CombinationSuccessful);
        session.SendSayi(ChatMessageColorType.Green, Game18NConstString.CombinationSuccessful);
        session.PlayerEntity.BroadcastEndDancingGuriPacket();
        session.SendShopEndPacket(ShopEndType.Item);
        session.SendShopEndPacket(ShopEndType.Npc);
    }
}