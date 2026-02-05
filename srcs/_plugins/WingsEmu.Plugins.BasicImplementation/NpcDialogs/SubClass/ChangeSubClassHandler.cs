// using System;
// using System.Threading.Tasks;
// using PhoenixLib.Configuration;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Characters.Events;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Character;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.SubClass;
//
// public class ChangeSubClassHandler : INpcDialogAsyncHandler
// {
//     private readonly IGameLanguageService _langService;
//
//     public ChangeSubClassHandler(IGameLanguageService langService) => _langService = langService;
//
//     public NpcRunType[] NpcRunTypes => [NpcRunType.CHOOSE_SUBCLASS];
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
//         SubClassType newSubClass = e.Argument switch
//         {
//             1 => SubClassType.OathKeeper,
//             2 => SubClassType.CrimsonFury,
//             3 => SubClassType.CelestialPaladin,
//             4 => SubClassType.SilentStalker,
//             5 => SubClassType.ArrowLord,
//             6 => SubClassType.ShadowHunter,
//             7 => SubClassType.ArcaneSage,
//             8 => SubClassType.Pyromancer,
//             9 => SubClassType.DarkNecromancer,
//             10 => SubClassType.ZenWarrior,
//             11 => SubClassType.EmperorsBlade,
//             12 => SubClassType.StealthShadow,
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
//         switch (e.Argument)
//         {
//             case 1 or 2 or 3 when session.PlayerEntity.Class != ClassType.Swordman:
//                 session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.Swordman));
//                 return;
//             case 4 or 5 or 6 when session.PlayerEntity.Class != ClassType.Archer:
//                 session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.Archer));
//                 return;
//             case 7 or 8 or 9 when session.PlayerEntity.Class != ClassType.Magician:
//                 session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.Magician));
//                 return;
//             case 10 or 11 or 12 when session.PlayerEntity.Class != ClassType.MartialArtist:
//                 session.SendInfo(_langService.GetLanguageFormat(GameDialogKey.CANNOT_CHANGE_SUBCLASS_WRONG_CLASS, session.UserLanguage, ClassType.MartialArtist));
//                 return;
//             case > 12:
//                 return;
//             default:
//                 switch (e.NpcRunType)
//                 {
//                     case NpcRunType.CHOOSE_SUBCLASS:
//                         await session.EmitEventAsync(new ChangeSubClassEvent
//                         {
//                             NewSubClass = newSubClass,
//                             ShouldObtainBasicItems = false,
//                             TierLevel = 1,
//                             TierExperience = 0
//                         });
//                         break;
//                 }
//                 
//                 string? newSubClassName = Enum.GetName(typeof(SubClassType), newSubClass)?.AddSpacesToCamelCase();
//                 session.SendChatMessageNoPlayer(session.GetLanguageFormat(GameDialogKey.SUCCESSFULLY_SUBCLASS_CHANGED, newSubClassName), ChatMessageColorType.Orange);
//                 break;
//         }
//     }
// }