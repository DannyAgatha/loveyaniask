// using System.Threading.Tasks;
// using WingsEmu.DTOs.Items;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Characters.Events;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Inventory;
// using WingsEmu.Game.Items;
// using WingsEmu.Game.Managers.StaticData;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Downgrade;
//
// public class DowngradeRarityHandler : INpcDialogAsyncHandler
// {
//     private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
//     private readonly IGameLanguageService _langService;
//     private readonly IItemsManager _itemsManager;
//     
//     public DowngradeRarityHandler(IGameLanguageService langService, IGameItemInstanceFactory gameItemInstanceFactory, IItemsManager itemsManager)
//     {
//         _langService = langService;
//         _gameItemInstanceFactory = gameItemInstanceFactory;
//         _itemsManager = itemsManager;
//     }
//     
//     public NpcRunType[] NpcRunTypes => [NpcRunType.DOWNGRADE_ITEM_RARITY];
//     
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
//         InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(0, InventoryType.Equipment);
//         
//         if (item == null)
//         {
//             session.SendChatMessage(session.GetLanguage(GameDialogKey.NEED_SPECIALIST_WEAPON_ARMOR_SP_FIRST_SLOT), ChatMessageColorType.Red);
//             session.SendMsg(session.GetLanguageFormat(GameDialogKey.NEED_SPECIALIST_WEAPON_ARMOR_SP_FIRST_SLOT), MsgMessageType.Middle);
//             return;
//         }
//         
//         if (item.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor && item.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon &&
//             item.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
//         {
//             session.SendChatMessage(session.GetLanguage(GameDialogKey.NEED_SPECIALIST_WEAPON_ARMOR_SP_FIRST_SLOT), ChatMessageColorType.Red);
//             session.SendMsg(session.GetLanguageFormat(GameDialogKey.NEED_SPECIALIST_WEAPON_ARMOR_SP_FIRST_SLOT), MsgMessageType.Middle);
//             return;
//         }
//         
//         if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
//         {
//             session.SendChatMessage(session.GetLanguage(GameDialogKey.NEED_SPECIALIST_WEAPON_ARMOR_SP_FIRST_SLOT), ChatMessageColorType.Red);
//             session.SendMsg(session.GetLanguageFormat(GameDialogKey.NEED_SPECIALIST_WEAPON_ARMOR_SP_FIRST_SLOT), MsgMessageType.Middle);
//             return;
//         }
//         
//         if (item.ItemInstance.Rarity != 8)
//         {
//             session.SendChatMessage(session.GetLanguage(GameDialogKey.NEED_RARITY_8_EQUIPMENT_FIRST_SLOT), ChatMessageColorType.Red);
//             session.SendMsg(session.GetLanguageFormat(GameDialogKey.NEED_RARITY_8_EQUIPMENT_FIRST_SLOT), MsgMessageType.Middle);
//             return;
//         }
//         
//         const long DowngradeCostGold = 5_000_000;
//         
//         if (session.PlayerEntity.Gold < DowngradeCostGold)
//         {
//             session.SendMsg(session.GetLanguageFormat(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), MsgMessageType.Middle);
//             return;
//         }
//         
//         short maxRarity = (short)(item.ItemInstance.GameItem.IsHeroic ? 8 : 7);
//         if (item.ItemInstance.Rarity >= maxRarity)
//         {
//             item.ItemInstance.Rarity -= 1;
//             session.SendInventoryAddPacket(item);
//             session.NotifyRarifyResult(_langService, item.ItemInstance.Rarity);
//             session.RefreshEquipment();
//             session.PlayerEntity.RemoveGold(DowngradeCostGold);
//             session.RefreshGold();
//             return;
//         }
//         
//         await session.EmitEventAsync(new ItemGambledEvent
//         {
//             ItemVnum = item.ItemInstance.ItemVNum,
//             Succeed = true,
//             FinalRarity = item.ItemInstance.Rarity
//         });
//     }
// }