// using System.Collections.Generic;
// using System.Threading.Tasks;
// using NosEmu.Plugins.BasicImplementations.Event.Items;
// using WingsAPI.Game.Extensions.ItemExtension.Inventory;
// using WingsAPI.Game.Extensions.PacketGeneration;
// using WingsAPI.Packets.Enums;
// using WingsAPI.Packets.Enums.Shells;
// using WingsEmu.DTOs.Items;
// using WingsEmu.Game;
// using WingsEmu.Game._enum;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Algorithm;
// using WingsEmu.Game.Characters.Events;
// using WingsEmu.Game.Configurations;
// using WingsEmu.Game.Entities;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Extensions.SubClass;
// using WingsEmu.Game.Inventory;
// using WingsEmu.Game.Inventory.Event;
// using WingsEmu.Game.Items;
// using WingsEmu.Game.Managers;
// using WingsEmu.Game.Networking;
// using WingsEmu.Game.Pity;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Downgrade;
//
// public class FastGamblingHandler : INpcDialogAsyncHandler
// {
//     private readonly IGamblingRarityConfiguration _gamblingRarityConfiguration;
//     private readonly GamblingRarityInfo _gamblingRarityInfo;
//     private readonly IGameLanguageService _gameLanguage;
//     private readonly IRandomGenerator _randomGenerator;
//     private readonly IServerManager _serverManager;
//     private readonly IShellGenerationAlgorithm _shellGenerationAlgorithm;
//     private readonly IEvtbConfiguration _evtbConfiguration;
//     private readonly PityConfiguration _pityConfiguration;
//     
//     public NpcRunType[] NpcRunTypes => [NpcRunType.FAST_GAMBLING];
//
//     public FastGamblingHandler(IGameLanguageService gameLanguage, IRandomGenerator randomGenerator,
//         IGamblingRarityConfiguration gamblingRarityConfiguration, GamblingRarityInfo gamblingRarityInfo,
//         IServerManager serverManager, IShellGenerationAlgorithm shellGenerationAlgorithm, IEvtbConfiguration evtbConfiguration, 
//         PityConfiguration pityConfiguration)
//     {
//         _gameLanguage = gameLanguage;
//         _randomGenerator = randomGenerator;
//         _gamblingRarityConfiguration = gamblingRarityConfiguration;
//         _gamblingRarityInfo = gamblingRarityInfo;
//         _serverManager = serverManager;
//         _shellGenerationAlgorithm = shellGenerationAlgorithm;
//         _evtbConfiguration = evtbConfiguration;
//         _pityConfiguration = pityConfiguration;
//     }
//
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
//         if (session.IsGeneralActionBlocked())
//         {
//             await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to use Fast Betting with general blocked action.");
//             return;
//         }
//         
//         INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
//         if (npcEntity == null)
//         {
//             return;
//         }
//         
//         InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(0, InventoryType.Equipment);
//         if (item == null)
//         {
//             session.SendInfo("Please place your equipment in the first slot of your inventory!");
//             return;
//         }
//         
//         if (item.ItemInstance.Rarity >= 8)
//         {
//             session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_ALREADY_MAX_RARE, session.UserLanguage), ChatMessageColorType.Yellow);
//             return;
//         }
//         
//         if (session.PlayerEntity.IsInExchange())
//         {
//             return;
//         }
//
//         if (session.PlayerEntity.HasShopOpened)
//         {
//             return;
//         }
//         
//         if (item.ItemInstance.GameItem.ItemType != ItemType.Weapon && item.ItemInstance.GameItem.ItemType != ItemType.Armor)
//         {
//             session.SendChatMessage("Please place an equipment in the first slot of your inventory!", ChatMessageColorType.Red);
//             return;
//         }
//
//         if (item.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Armor && 
//             item.ItemInstance.GameItem.EquipmentSlot != EquipmentType.MainWeapon &&
//             item.ItemInstance.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
//         {
//             session.SendChatMessage("Please place your equipment in the first slot of your inventory!", ChatMessageColorType.Red);
//             return;
//         }
//
//         if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
//         {
//             session.SendChatMessage("Please ensure a wearable equipment is in the first slot of your inventory!", ChatMessageColorType.Red);
//             return;
//         }
//         
//         while (Betting(session, item))
//         {
//             session.PlayerEntity.RemoveGold(500);
//             session.RefreshGold();
//             await session.RemoveItemFromInventory((short)ItemVnums.CELLA, 5);
//         }
//     }
//     
//     private readonly List<int> _amuletVnums = [(short)ItemVnums.BLESSING_AMULET, (short)ItemVnums.PROTECTION_AMULET, (short)ItemVnums.BLESSING_AMULET_DOUBLE];
//     private readonly List<int> _heroAmulets = [(short)ItemVnums.CHAMPION_AMULET, (short)ItemVnums.CHAMPION_AMULET_RANDOM];
//
//     private bool Betting(IClientSession session, InventoryItem item)
//     {
//         if (session.PlayerEntity.CountItemWithVnum((short)ItemVnums.CELLA) < 5)
//         {
//             session.SendChatMessage("You need at least 5 Cellas to proceed!", ChatMessageColorType.Red);
//             return false;
//         }
//
//         if (session.PlayerEntity.Gold < 500)
//         {
//             session.SendChatMessage("You need at least 500 gold to proceed!", ChatMessageColorType.Red);
//             return false;
//         }
//
//         
//         InventoryItem amulet = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(EquipmentType.Amulet);
//         if (amulet == null)
//         {
//             bool found = false;
//             if (item.ItemInstance.GameItem.IsHeroic)
//             {
//                 foreach (int newVnum in _heroAmulets)
//                 {
//                     if (!session.PlayerEntity.HasItem(newVnum))
//                     {
//                         continue;
//                     }
//
//                     amulet = session.PlayerEntity.GetFirstItemByVnum(newVnum);
//                     session.EmitEventAsync(new InventoryEquipItemEvent(amulet.Slot));
//                     found = true;
//                     break;
//                 }
//             }
//             else
//             {
//                 foreach (int newVnum in _amuletVnums)
//                 {
//                     if (!session.PlayerEntity.HasItem(newVnum))
//                     {
//                         continue;
//                     }
//
//                     amulet = session.PlayerEntity.GetFirstItemByVnum(newVnum);
//                     session.EmitEventAsync(new InventoryEquipItemEvent(amulet.Slot));
//                     found = true;
//
//                     break;
//                 }
//             }
//
//             if (!found)
//             {
//                 session.SendInfo("An amulet is required to do that!");
//                 return false;
//             }
//         }
//         RarifyProtection protection = RarifyProtection.None;
//         switch (amulet.ItemInstance.ItemVNum)
//         {
//             case (int)ItemVnums.BLESSING_AMULET:
//                 protection = RarifyProtection.BlessingAmulet;
//                 break;
//             case (int)ItemVnums.PROTECTION_AMULET:
//                 protection = RarifyProtection.ProtectionAmulet;
//                 break;
//             case (int)ItemVnums.BLESSING_AMULET_DOUBLE:
//                 protection = RarifyProtection.BlessingAmulet;
//                 break;
//             case (int)ItemVnums.CHAMPION_AMULET:
//                 protection = RarifyProtection.HeroicAmulet;
//                 break;
//             case (int)ItemVnums.CHAMPION_AMULET_RANDOM:
//                 protection = RarifyProtection.RandomHeroicAmulet;
//                 break;
//         }
//         
//         if (protection == RarifyProtection.None)
//         {
//             return false;
//         }
//
//         short originalRarity = item.ItemInstance.Rarity;
//         short maxRarity = (short)(item.ItemInstance.GameItem.IsHeroic ? 8 : 7);
//         bool isSuccess = GamblingSuccess(item.ItemInstance, amulet.ItemInstance);
//         bool forceMaxRarity = false;
//         
//         if (!isSuccess)
//         {
//             if (item.ItemInstance.IsPityUpgradeItem(PityType.Betting, _pityConfiguration))
//             {
//                 item.ItemInstance.PityCounter[(int)PityType.Betting] = 0;
//                 forceMaxRarity = true;
//                 isSuccess = true;
//                 int basePoints = session.PlayerEntity.SubClass.IsPveSubClass() ? 10 : session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 5 : 0;
//                 session.AddTierExperience(basePoints, _gameLanguage, handleExpTimer: false);
//             }
//             else
//             {
//                 item.ItemInstance.PityCounter[(int)PityType.Betting]++;
//             }
//         }
//
//         if (isSuccess)
//         {
//             short rarity = _gamblingRarityConfiguration.GetRandomRarity();
//             
//             if (rarity > maxRarity)
//             {
//                 rarity = maxRarity;
//             }
//            
//             bool isMaxRare = rarity switch
//             {
//                 7 when !item.ItemInstance.GameItem.IsHeroic => true,
//                 8 => true,
//                 _ => false
//             };
//            
//             switch (isMaxRare)
//             {
//                 case true:
//                     item.ItemInstance.PityCounter[(int)PityType.Betting] = 0;
//                     break;
//                 case false when forceMaxRarity:
//                     rarity = (short)(item.ItemInstance.GameItem.IsHeroic ? 8 : 7);
//                     break;
//             }
//             
//             if (protection is RarifyProtection.RandomHeroicAmulet or RarifyProtection.BlessingAmulet)
//             {
//                 ShellType shellType = item.ItemInstance.GameItem.ItemType == ItemType.Armor ? ShellType.PvpShellArmor : ShellType.PvpShellWeapon;
//                 
//                 if (item.ItemInstance.EquipmentOptions != null && item.ItemInstance.EquipmentOptions.Count != 0)
//                 {
//                     item.ItemInstance.EquipmentOptions.Clear();
//                     item.ItemInstance.ShellRarity = null;
//                 }
//                 
//                 IEnumerable<EquipmentOptionDTO> shellOptions = _shellGenerationAlgorithm.GenerateShell((byte)shellType, rarity, 99);
//                 item.ItemInstance.EquipmentOptions = [];
//                 item.ItemInstance.EquipmentOptions.AddRange(shellOptions);
//                 session.NotifyRarifyResult(_gameLanguage, rarity);
//                 session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
//                 item.ItemInstance.Rarity = rarity;
//             }
//             else
//             {
//                 session.NotifyRarifyResult(_gameLanguage, rarity);
//                 session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
//                 item.ItemInstance.Rarity = rarity;
//             }
//
//             item.ItemInstance.SetRarityPoint(_randomGenerator);
//             session.SendInventoryAddPacket(item);
//
//             session.EmitEventAsync(new ItemGambledEvent
//             {
//                 ItemVnum = item.ItemInstance.ItemVNum,
//                 Mode = RarifyMode.Normal,
//                 Protection = protection,
//                 Amulet = amulet.ItemInstance.ItemVNum,
//                 Succeed = true,
//                 OriginalRarity = originalRarity,
//                 FinalRarity = item.ItemInstance.Rarity
//             });
//
//             if (rarity < 7)
//             {
//                 return true;
//             }
//
//             session.SendChatMessage("Your item has reached maximum rarity!", ChatMessageColorType.Yellow);
//             return false;
//         }
//
//         switch (protection)
//         {
//             case RarifyProtection.ProtectionAmulet:
//             case RarifyProtection.BlessingAmulet:
//             case RarifyProtection.HeroicAmulet:
//             case RarifyProtection.RandomHeroicAmulet:
//
//                 amulet.ItemInstance.DurabilityPoint -= 1;
//                 session.SendAmuletBuffPacket(amulet.ItemInstance);
//
//                 session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_AMULET_FAIL_SAVED, session.UserLanguage), ChatMessageColorType.Red);
//                 session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_AMULET_FAIL_SAVED, session.UserLanguage), MsgMessageType.Middle);
//
//                 session.EmitEventAsync(new ItemGambledEvent
//                 {
//                     ItemVnum = item.ItemInstance.ItemVNum,
//                     Mode = RarifyMode.Normal,
//                     Protection = protection,
//                     Amulet = amulet.ItemInstance.ItemVNum,
//                     Succeed = false,
//                     OriginalRarity = originalRarity,
//                     FinalRarity = item.ItemInstance.Rarity
//                 });
//                 
//                 if (amulet.ItemInstance.DurabilityPoint > 0)
//                 {
//                     return true;
//                 }
//
//                 session.RemoveItemFromInventory(item: amulet, isEquiped: true).ConfigureAwait(false).GetAwaiter().GetResult();
//                 session.RefreshEquipment();
//                 session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_INFO_AMULET_DESTROYED, session.UserLanguage), ModalType.Confirm);
//                 return true;
//         }
//         return true;
//     }
//
//     private ShellType GetRandomShell(int rare, ItemType type)
//     {
//         if (type == ItemType.Armor)
//         {
//             var shellTypes = new List<ShellType>()
//                 { ShellType.FullShellArmor, ShellType.PerfectShellArmor, ShellType.HalfShellArmor, ShellType.PvpShellArmor, ShellType.SpecialShellArmor };
//             int rnd = _randomGenerator.RandomNumber(shellTypes.Count);
//             ShellType rng = shellTypes[rnd];
//             return rng;
//         }
//         else
//         {
//             var shellTypes = new List<ShellType>() { ShellType.FullShellWeapon, ShellType.PerfectShellWeapon, ShellType.HalfShellWeapon, ShellType.PvpShellWeapon, ShellType.SpecialShellWeapon };
//             int rnd = _randomGenerator.RandomNumber(shellTypes.Count);
//             ShellType rng = shellTypes[rnd];
//             return rng;
//         }
//     }
//
//     private bool GamblingSuccess(GameItemInstance item, GameItemInstance amulet)
//     {
//         if (item.Rarity < 0)
//         {
//             return true;
//         }
//
//         RaritySuccess raritySuccess = _gamblingRarityConfiguration.GetRaritySuccess((byte)item.Rarity);
//         if (raritySuccess == null)
//         {
//             return false;
//         }
//         
//         int rnd = _randomGenerator.RandomNumber(10000);
//         return rnd < (IsEnhanced(amulet) ? raritySuccess.SuccessChance + 1000 : raritySuccess.SuccessChance) + (int)(raritySuccess.SuccessChance * _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_GAMBLING_EQUIPMENT) * 0.01);
//     }
//
//     private bool IsChampion(GameItemInstance amulet) =>
//         amulet.ItemVNum is (short)ItemVnums.CHAMPION_AMULET or (short)ItemVnums.CHAMPION_AMULET_RANDOM or (short)ItemVnums.CHAMPION_AMULET_INCREASE_1;
//
//     private bool IsEnhanced(GameItemInstance amulet) =>
//         amulet != null && amulet.ItemVNum is (short)ItemVnums.BLESSING_AMULET or (short)ItemVnums.BLESSING_AMULET_DOUBLE or (short)ItemVnums.CHAMPION_AMULET or (short)ItemVnums.CHAMPION_AMULET_RANDOM
//             or (short)ItemVnums.PROTECTION_AMULET;
// }