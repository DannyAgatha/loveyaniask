// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using WingsAPI.Game.Extensions.ItemExtension.Inventory;
// using WingsAPI.Game.Extensions.ItemExtension.Item;
// using WingsEmu.Game;
// using WingsEmu.Game._enum;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Entities;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Items;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Random;
//
// public class DiceOfDestinyHandler : INpcDialogAsyncHandler
// {
//     private readonly IRandomGenerator _randomGenerator;
//     private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
//     private readonly IGameLanguageService _languageService;
//
//     public DiceOfDestinyHandler(IRandomGenerator randomGenerator, IGameItemInstanceFactory gameItemInstanceFactory, IGameLanguageService languageService)
//     {
//         _gameItemInstanceFactory = gameItemInstanceFactory;
//         _randomGenerator = randomGenerator;
//         _languageService = languageService;
//     }
//
//     public NpcRunType[] NpcRunTypes => new[]
//     {
//         NpcRunType.DICE_DESTINY_GOLD
//     };
//     
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
//         INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
//         if (npcEntity == null)
//         {
//             return;
//         }
//
//         const int exchangeCost = 10000000; // Establecer el costo del intercambio en 10 millones
//
//         int randomNumber = _randomGenerator.RandomNumber();
//         bool isEven = (randomNumber % 2) == 0; // Verificar si el número es par
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
//         session.SendMsg(_languageService.GetLanguage(GameDialogKey.DICE_OF_DESTINY_PREPARING, session.UserLanguage), MsgMessageType.Middle);
//
//         await Task.Delay(TimeSpan.FromSeconds(3)); // Pausa de 3 segundos
//         
//         session.SendMsg(_languageService.GetLanguage(GameDialogKey.DICE_OF_DESTINY_START, session.UserLanguage), MsgMessageType.Middle);
//
//         await Task.Delay(TimeSpan.FromSeconds(4)); // Pausa de 4 segundos
//         
//         session.SendMsg(_languageService.GetLanguage(GameDialogKey.DICE_OF_DESTINY_END, session.UserLanguage), MsgMessageType.Middle);
//         
//         await Task.Delay(TimeSpan.FromSeconds(5)); // Pausa de 5 segundos
//
//         if (isEven)
//         {
//             session.SendEffect(EffectType.DoubleChanceDrop);
//             var vnums = new List<(long, int)>
//             {
//                 (2333, 20), // Giant Lump Gold
//                 (2333, 25), // Giant Lump Gold
//                 (2333, 30), // Giant Lump Gold
//                 (30028, 1), // Dracula Hat
//                 (30033, 1), // Dracula Costume
//                 (30752, 1), // Wonderland Hat
//                 (30758, 1), // Wonderland Costume
//                 (4405, 1), // Yuna PSP
//                 (5998, 1), // Bone Drake
//                 (5560, 1), // Onyx Wings
//                 (5800, 1), // Lightning Wings
//                 (5837, 1), // Mega Titan Wings
//                 (5431, 1), // Archangel Wings
//                 (5432, 1), // Archdaemon Wings
//                 (5498, 1), // Blazing Fire Wings
//                 (5499, 1), // Frosty Ice Wings
//                 (4129, 1), // Rumial
//                 (4130, 1), // Ladine
//                 (4131, 1), // Rumial
//                 (4132, 1), // Varik
//                 (4304, 1), // Frigg
//                 (4305, 1), // Ragnar
//                 (4306, 1), // Erdimien
//                 (4315, 1), // Jennifer
//                 (4464, 1), // Black Ink Rabbit
//                 (4407, 1), // Polar Bear
//                 (4676, 1), // Sleepy Koala
//                 (8511, 1), // Panda
//                 (5893, 1), // Mysterious Medal
//                 (5892, 2), // Legendary Medal
//                 (4547, 1), // Akhenaton the Cursed Pharaoh
//                 (4446, 1), // One-Winged Perti Specialist Partner Card
//                 (4808, 1), // Maru's Specialist Partner Card
//                 (4824, 1), // Nelia Nymph
//                 (30642, 5), // Random Shell Box
//                 (30642, 8), // Random Shell Box
//                 (30642, 10), // Random Shell Box
//                 (2282, 99), // Angel Feather
//                 (1030, 50), // Full Moon
//                 (2283, 50), // Green Soul
//                 (2284, 30), // Red Soul
//                 (2285, 25), // Blue Soul
//                 (2511, 20), // Dragon Skin
//                 (2512, 15), // Dragon Blood
//                 (2513, 10), // Dragon Heart
//                 (1366, 2), // Init Potion
//                 (1904, 5), // Tarot Card Game
//                 (4240, 1), // Golden Specialist Card
//                 (5369, 5), // Golden Equipment Protection 
//                 (30652, 5), // Astral Points
//                 (30652, 10), // Astral Points
//                 (30652, 15), // Astral Points
//                 (30652, 20), // Astral Points
//                 (2514, 25), // Small Ruby of Completion
//                 (2515, 25), // Small Sapphire of Completion
//                 (2516, 25), // Small Obsidian of Completion
//                 (2517, 25), // Small Topaz of Completion
//                 (2518, 25), // Ruby of Completion
//                 (2519, 25), // Sapphire of Completion
//                 (2520, 25), // Obsidian of Completion
//                 (2521, 25), // Topaz of Completion
//                 (2514, 50), // Small Ruby of Completion
//                 (2515, 50), // Small Sapphire of Completion
//                 (2516, 50), // Small Obsidian of Completion
//                 (2517, 50), // Small Topaz of Completion
//                 (2518, 50), // Ruby of Completion
//                 (2519, 50), // Sapphire of Completion
//                 (2520, 50), // Obsidian of Completion
//                 (2521, 50), // Topaz of Completion
//                 (5931, 5), // Partner Skill Ticket (Single)
//                 (5932, 10), // Partner Skill Ticket (All)
//             };
//
//             int randomIndex = _randomGenerator.RandomNumber(0, vnums.Count);
//             (long itemVNum, int itemAmount) = vnums[randomIndex];
//
//             GameItemInstance itemInstance = _gameItemInstanceFactory.CreateItem((int)itemVNum, itemAmount);
//             await session.AddNewItemToInventory(itemInstance, true, ChatMessageColorType.Yellow, true);
//             session.SendRdiPacket(itemInstance.ItemVNum, (short)itemInstance.Amount);
//             session.SendInfo(_languageService.GetLanguageFormat(GameDialogKey.DICE_OF_DESTINY_SUCCESS, session.UserLanguage, randomNumber, itemAmount,
//                 itemInstance.GameItem.GetItemName(_languageService, session.UserLanguage)));
//         }
//         else
//         {
//             session.SendEffect(EffectType.PetLoveBroke);
//             session.SendInfo(_languageService.GetLanguageFormat(GameDialogKey.DICE_OF_DESTINY_LOSSE, session.UserLanguage, randomNumber));
//         }
//     }
// }