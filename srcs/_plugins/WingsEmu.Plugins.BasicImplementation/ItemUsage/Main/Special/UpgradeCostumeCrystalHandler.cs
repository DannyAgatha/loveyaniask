// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using WingsAPI.Game.Extensions.ItemExtension.Inventory;
// using WingsAPI.Packets.Enums;
// using WingsEmu.Game;
// using WingsEmu.Game._enum;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._ItemUsage;
// using WingsEmu.Game._ItemUsage.Event;
// using WingsEmu.Game.Configurations.UpgradeCostume;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Inventory;
// using WingsEmu.Game.Items;
// using WingsEmu.Game.Managers.StaticData;
// using WingsEmu.Game.Networking;
// using WingsEmu.Game.Pity;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;
//
// public class UpgradeCostumeCrystalHandler : IItemHandler
// {
//     private readonly IGameLanguageService _gameLanguage;
//     private readonly IRandomGenerator _randomGenerator;
//     private readonly FashionUpgradeConfiguration _fashionUpgradeConfiguration;
//     private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
//     private readonly PityConfiguration _pityConfiguration;
//     private readonly IItemsManager _itemsManager;
//
//     public UpgradeCostumeCrystalHandler(IGameLanguageService gameLanguage, IRandomGenerator randomGenerator, FashionUpgradeConfiguration fashionUpgradeConfiguration,
//         IGameItemInstanceFactory gameItemInstanceFactory, PityConfiguration pityConfiguration, IItemsManager itemsManager)
//     {
//         _gameLanguage = gameLanguage;
//         _randomGenerator = randomGenerator;
//         _fashionUpgradeConfiguration = fashionUpgradeConfiguration;
//         _gameItemInstanceFactory = gameItemInstanceFactory;
//         _pityConfiguration = pityConfiguration;
//         _itemsManager = itemsManager;
//     }
//
//     public ItemType ItemType => ItemType.Special;
//     public long[] Effects => [20000];
//
//     public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
//     {
//         if (!byte.TryParse(e.Packet[9], out byte islot))
//         {
//             return;
//         }
//
//         InventoryItem wearInstance = session.PlayerEntity.GetItemBySlotAndType(islot, InventoryType.Equipment);
//
//         if (wearInstance == null || wearInstance.ItemInstance.GameItem.ItemType != ItemType.Fashion)
//         {
//             return;
//         }
//
//         FashionUpgradeSet fashionUpgradeSet = _fashionUpgradeConfiguration.FashionUpgrade
//             .SelectMany(f => f.Sets)
//             .FirstOrDefault(set => set.RequiredFashionVnum == wearInstance.ItemInstance.ItemVNum);
//
//         if (fashionUpgradeSet == null)
//         {
//             session.SendInfo(session.GetLanguageFormat(GameDialogKey.FASHION_EVOLUTION_ERROR));
//             return;
//         }
//
//         if (!session.HasEnoughGold(10_000_000))
//         {
//             session.SendInfoi(Game18NConstString.NotEnoughFounds);
//             return;
//         }
//
//         session.PlayerEntity.Gold -= 10_000_000;
//         session.RefreshGold();
//
//         if (!await HasRequiredMaterials(session, fashionUpgradeSet.Materials))
//         {
//             session.SendInfo(session.GetLanguageFormat(GameDialogKey.FASHION_EVOLUTION_ENOUGH_MATERIALS));
//             return;
//         }
//
//         await RemoveRequiredMaterials(session, fashionUpgradeSet.Materials);
//         await session.RemoveItemFromInventory(item: e.Item);
//
//         bool success = _randomGenerator.RandomNumber(0, 100) <= fashionUpgradeSet.Chance;
//
//         if (success)
//         {
//             await HandleUpgradeSuccess(session, wearInstance, fashionUpgradeSet);
//         }
//         else
//         {
//             await HandleUpgradeFailure(session, wearInstance, fashionUpgradeSet);
//         }
//     }
//
//     private async Task HandleUpgradeSuccess(IClientSession session, InventoryItem wearInstance, FashionUpgradeSet fashionUpgradeSet)
//     {
//         wearInstance.ItemInstance.PityCounter[(int)PityType.Fashion] = 0;
//
//         GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(fashionUpgradeSet.ObtainedFashionVnum, 1);
//         await session.AddNewItemToInventory(newItem, true, ChatMessageColorType.Yellow, true);
//         session.SendRdiPacket(newItem.ItemVNum, (short)newItem.Amount);
//         await session.RemoveItemFromInventory(wearInstance.ItemInstance.ItemVNum);
//         session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
//         
//         string itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(wearInstance.ItemInstance.ItemVNum), session);
//         session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.FASHION_EVOLUTION_SUCCESS, session.UserLanguage, itemName), ChatMessageColorType.Green);
//         session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.FASHION_EVOLUTION_SUCCESS, session.UserLanguage, itemName), MsgMessageType.Middle);
//     }
//
//     private async Task HandleUpgradeFailure(IClientSession session, InventoryItem wearInstance, FashionUpgradeSet fashionUpgradeSet)
//     {
//         if (wearInstance.ItemInstance.IsPityUpgradeItem(PityType.Fashion, _pityConfiguration))
//         {
//             wearInstance.ItemInstance.PityCounter[(int)PityType.Fashion] = 0;
//             session.SendChatMessage(session.GetLanguage(GameDialogKey.PITY_CHATMESSAGE_SUCCESS), ChatMessageColorType.Green);
//             await HandleUpgradeSuccess(session, wearInstance, fashionUpgradeSet);
//         }
//         else
//         {
//             wearInstance.ItemInstance.PityCounter[(int)PityType.Fashion]++;
//             (int, int) maxFailCounter = wearInstance.ItemInstance.ItemPityMaxFailCounter(PityType.Fashion, _pityConfiguration);
//             session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PITY_CHATMESSAGE_FAIL, maxFailCounter.Item1, maxFailCounter.Item2),
//                 ChatMessageColorType.Green);
//
//             session.BroadcastEffectInRange(EffectType.UpgradeFail);
//             session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.FASHION_EVOLUTION_FAIL, session.UserLanguage), ChatMessageColorType.Red);
//             session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.FASHION_EVOLUTION_FAIL, session.UserLanguage), MsgMessageType.Middle);
//         }
//     }
//
//
//     private async Task<bool> HasRequiredMaterials(IClientSession session, List<Material> materials)
//     {
//         return materials.All(material => session.PlayerEntity.HasItem(material.ItemVnum, material.Amount));
//     }
//
//     private async Task RemoveRequiredMaterials(IClientSession session, List<Material> materials)
//     {
//         foreach (Material material in materials)
//         {
//             await session.RemoveItemFromInventory(material.ItemVnum, material.Amount);
//         }
//     }
// }