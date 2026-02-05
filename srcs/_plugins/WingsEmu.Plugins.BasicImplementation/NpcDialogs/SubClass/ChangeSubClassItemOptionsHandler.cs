// using System;
// using System.Threading.Tasks;
// using PhoenixLib.Configuration;
// using WingsAPI.Game.Extensions.ItemExtension.Inventory;
// using WingsAPI.Game.Extensions.ItemExtension.Item;
// using WingsEmu.Game._enum;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Characters.Events;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Managers.StaticData;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Character;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.SubClass;
//
// public class ChangeSubClassItemOptionsHandler : INpcDialogAsyncHandler
// {
//     private readonly IGameLanguageService _langService;
//     private readonly IItemsManager _itemsManager;
//
//     public ChangeSubClassItemOptionsHandler(IGameLanguageService langService, IItemsManager itemsManager)
//     {
//         _langService = langService;
//         _itemsManager = itemsManager;
//     }
//
//     public NpcRunType[] NpcRunTypes =>
//     [
//         NpcRunType.CHANGE_SUBCLASS_BASIC_SWORDMAN,
//         NpcRunType.CHANGE_SUBCLASS_BASIC_ARCHER,
//         NpcRunType.CHANGE_SUBCLASS_BASIC_MAGE,
//         NpcRunType.CHANGE_SUBCLASS_BASIC_MARTIAL_ARTIST,
//         NpcRunType.CHANGE_SUBCLASS_PREMIUM_SWORDMAN,
//         NpcRunType.CHANGE_SUBCLASS_PREMIUM_ARCHER,
//         NpcRunType.CHANGE_SUBCLASS_PREMIUM_MAGE,
//         NpcRunType.CHANGE_SUBCLASS_PREMIUM_MARTIAL_ARTIST
//     ];
//
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
//         string? subClassName = Enum.GetName(typeof(SubClassType), session.PlayerEntity.SubClass);
//
//         if (subClassName == null)
//         {
//             return;
//         }
//
//         subClassName = subClassName.AddSpacesToCamelCase();
//
//         SubClassType newSubClass = e.NpcRunType switch
//         {
//             NpcRunType.CHANGE_SUBCLASS_BASIC_SWORDMAN or NpcRunType.CHANGE_SUBCLASS_PREMIUM_SWORDMAN => e.Argument switch
//             {
//                 0 => SubClassType.OathKeeper,
//                 1 => SubClassType.CrimsonFury,
//                 2 => SubClassType.CelestialPaladin
//             },
//             NpcRunType.CHANGE_SUBCLASS_BASIC_ARCHER or NpcRunType.CHANGE_SUBCLASS_PREMIUM_ARCHER => e.Argument switch
//             {
//                 0 => SubClassType.SilentStalker,
//                 1 => SubClassType.ArrowLord,
//                 2 => SubClassType.ShadowHunter
//             },
//             NpcRunType.CHANGE_SUBCLASS_BASIC_MAGE or NpcRunType.CHANGE_SUBCLASS_PREMIUM_MAGE => e.Argument switch
//             {
//                 0 => SubClassType.ArcaneSage,
//                 1 => SubClassType.Pyromancer,
//                 2 => SubClassType.DarkNecromancer
//             },
//             NpcRunType.CHANGE_SUBCLASS_BASIC_MARTIAL_ARTIST or NpcRunType.CHANGE_SUBCLASS_PREMIUM_MARTIAL_ARTIST => e.Argument switch
//             {
//                 0 => SubClassType.ZenWarrior,
//                 1 => SubClassType.EmperorsBlade,
//                 2 => SubClassType.StealthShadow
//             }
//         };
//
//         if (session.CantPerformActionOnAct4())
//         {
//             return;
//         }
//
//         if (session.PlayerEntity.Level < 15 || session.PlayerEntity.JobLevel < 20)
//         {
//             session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_TOO_LOW_LVL, session.UserLanguage), MsgMessageType.Middle);
//             return;
//         }
//
//         if (session.PlayerEntity.IsInGroup())
//         {
//             session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NEED_LEAVE_GROUP, session.UserLanguage), MsgMessageType.Middle);
//             return;
//         }
//
//         if (session.PlayerEntity.SubClass != (int)SubClassType.NotDefined && e.NpcRunType == NpcRunType.CHOOSE_SUBCLASS)
//         {
//             session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.ALREADY_HAVE_SUBCLASS, session.UserLanguage, subClassName));
//             return;
//         }
//         
//         if (session.PlayerEntity.SubClass == newSubClass)
//         {
//             session.SendMsg(_langService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_SAME_SUBCLASS, session.UserLanguage), MsgMessageType.Middle);
//             return;
//         }
//
//         switch (e.NpcRunType)
//         {
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_SWORDMAN or NpcRunType.CHANGE_SUBCLASS_PREMIUM_SWORDMAN:
//                 if (session.PlayerEntity.Class != ClassType.Swordman)
//                 {
//                     session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.Swordman));
//                     return;
//                 }
//
//                 break;
//
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_ARCHER or NpcRunType.CHANGE_SUBCLASS_PREMIUM_ARCHER:
//                 if (session.PlayerEntity.Class != ClassType.Archer)
//                 {
//                     session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.Archer));
//                     return;
//                 }
//
//                 break;
//
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_MAGE or NpcRunType.CHANGE_SUBCLASS_PREMIUM_MAGE:
//                 if (session.PlayerEntity.Class != ClassType.Magician)
//                 {
//                     session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.Magician));
//                     return;
//                 }
//
//                 break;
//
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_MARTIAL_ARTIST or NpcRunType.CHANGE_SUBCLASS_PREMIUM_MARTIAL_ARTIST:
//                 if (session.PlayerEntity.Class != ClassType.MartialArtist)
//                 {
//                     session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.MartialArtist));
//                     return;
//                 }
//
//                 break;
//         }
//
//         switch (e.NpcRunType)
//         {
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_SWORDMAN:
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_ARCHER:
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_MAGE:
//             case NpcRunType.CHANGE_SUBCLASS_BASIC_MARTIAL_ARTIST:
//                 if (!session.PlayerEntity.HasItem((short)ItemVnums.SUBCLASS_CHANGER_BASIC))
//                 {
//                     string itemName = _itemsManager.GetItem((short)ItemVnums.SUBCLASS_CHANGER_BASIC).GetItemName(_langService, session.UserLanguage);
//                     session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, 1, itemName), ChatMessageColorType.PlayerSay);
//                     return;
//                 }
//
//                 await session.EmitEventAsync(new ChangeSubClassEvent
//                 {
//                     NewSubClass = newSubClass,
//                     ShouldObtainBasicItems = false,
//                     TierLevel = 1,
//                     TierExperience = 0
//                 });
//                 await session.RemoveItemFromInventory((short)ItemVnums.SUBCLASS_CHANGER_BASIC);
//                 break;
//
//             case NpcRunType.CHANGE_SUBCLASS_PREMIUM_SWORDMAN:
//             case NpcRunType.CHANGE_SUBCLASS_PREMIUM_ARCHER:
//             case NpcRunType.CHANGE_SUBCLASS_PREMIUM_MAGE:
//             case NpcRunType.CHANGE_SUBCLASS_PREMIUM_MARTIAL_ARTIST:
//                 if (!session.PlayerEntity.HasItem((short)ItemVnums.SUBCLASS_CHANGER_PREMIUM))
//                 {
//                     string itemName = _itemsManager.GetItem((short)ItemVnums.SUBCLASS_CHANGER_PREMIUM).GetItemName(_langService, session.UserLanguage);
//                     session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, 1, itemName), ChatMessageColorType.PlayerSay);
//                     return;
//                 }
//
//                 await session.EmitEventAsync(new ChangeSubClassEvent
//                 {
//                     NewSubClass = newSubClass,
//                     ShouldObtainBasicItems = false,
//                     TierLevel = session.PlayerEntity.TierLevel,
//                     TierExperience = session.PlayerEntity.TierExperience
//                 });
//                 await session.RemoveItemFromInventory((short)ItemVnums.SUBCLASS_CHANGER_PREMIUM);
//                 break;
//         }
//
//         string? newSubClassName = Enum.GetName(typeof(SubClassType), newSubClass)?.AddSpacesToCamelCase();
//         session.SendChatMessageNoPlayer(session.GetLanguageFormat(GameDialogKey.SUCCESSFULLY_SUBCLASS_CHANGED, newSubClassName), ChatMessageColorType.Orange);
//     }
// }