// using System;
// using System.Threading.Tasks;
// using WingsAPI.Game.Extensions.ItemExtension.Inventory;
// using WingsEmu.Game;
// using WingsEmu.Game._enum;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Items;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Random;
//
// public class PartnerSpecialistFragmentsHandler : INpcDialogAsyncHandler
// {
//     private readonly IRandomGenerator _randomGenerator;
//     private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
//     private readonly IGameLanguageService _languageService;
//
//     public PartnerSpecialistFragmentsHandler(IRandomGenerator randomGenerator, IGameItemInstanceFactory gameItemInstanceFactory, IGameLanguageService languageService)
//     {
//         _gameItemInstanceFactory = gameItemInstanceFactory;
//         _randomGenerator = randomGenerator;
//         _languageService = languageService;
//     }
//
//     public NpcRunType[] NpcRunTypes => new[]
//     {
//         NpcRunType.PSP_FRAGMENT_TIER_EXCHANGER
//     };
//
//     private (long, int) SelectFragment()
//     {
//         int randomNumber = _randomGenerator.RandomNumber(0, 100);
//         int fragmentAmount;
//     
//         switch (randomNumber)
//         {
//             // 50% de probabilidad
//             case < 50:
//             {
//                 int[] allowedAmounts = { 10, 15, 20 };  // Cantidades permitidas
//                 fragmentAmount = allowedAmounts[_randomGenerator.RandomNumber(0, allowedAmounts.Length)];
//                 return (30896, fragmentAmount);  // PSP Fragment Tier C
//             }
//             // 30% de probabilidad
//             case < 80:
//             {
//                 int[] allowedAmounts = { 5, 10, 15, 20 };  // Cantidades permitidas
//                 fragmentAmount = allowedAmounts[_randomGenerator.RandomNumber(0, allowedAmounts.Length)];
//                 return (30897, fragmentAmount);  // PSP Fragment Tier B
//             }
//             // 15% de probabilidad
//             case < 95:
//             {
//                 int[] allowedAmounts = { 5, 10 };  // Cantidades permitidas
//                 fragmentAmount = allowedAmounts[_randomGenerator.RandomNumber(0, allowedAmounts.Length)];
//                 return (30898, fragmentAmount);  // PSP Fragment Tier A
//             }
//             // 5% de probabilidad
//             default:
//             {
//                 int[] allowedAmounts = { 5 };  // Cantidades permitidas
//                 fragmentAmount = allowedAmounts[_randomGenerator.RandomNumber(0, allowedAmounts.Length)];
//                 return (30899, fragmentAmount);  // PSP Fragment Tier S
//             }
//         }
//     }
//
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
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
//         const int exchangeCost = 5000000;
//
//         if (session.PlayerEntity.Gold < exchangeCost)
//         {
//             session.SendMsg(session.GetLanguageFormat(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), MsgMessageType.Middle);
//             return;
//         }
//
//         session.PlayerEntity.RemoveGold(exchangeCost);
//         session.RefreshGold();
//         
//         session.SendMsg(_languageService.GetLanguage(GameDialogKey.PARTNER_PSP_EXCHANGER_PREPARING, session.UserLanguage), MsgMessageType.Middle);
//         await Task.Delay(TimeSpan.FromSeconds(3));
//
//         (long itemVNum, int itemAmount) = SelectFragment();
//
//         session.SendEffect(EffectType.DoubleChanceDrop);
//         GameItemInstance itemInstance = _gameItemInstanceFactory.CreateItem((int)itemVNum, itemAmount);
//         await session.AddNewItemToInventory(itemInstance, true, ChatMessageColorType.Yellow, true);
//         session.SendRdiPacket(itemInstance.ItemVNum, (short)itemInstance.Amount);
//     }
// }