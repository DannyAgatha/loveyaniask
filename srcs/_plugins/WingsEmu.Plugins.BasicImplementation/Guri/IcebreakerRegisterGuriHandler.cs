// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using WingsAPI.Packets.Enums.Chat;
// using WingsEmu.Game._Guri;
// using WingsEmu.Game._Guri.Event;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Icebreaker;
// using WingsEmu.Game.Networking;
// using WingsEmu.Game.RainbowBattle;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.Guri;
//
// public class IcebreakerRegisterGuriHandler : IGuriHandler
// {
//     private readonly IIcebreakerManager _icebreakerManager;
//
//     public IcebreakerRegisterGuriHandler(IIcebreakerManager icebreakerManager) => _icebreakerManager = icebreakerManager;
//
//     public long GuriEffectId => 501;
//
//     public async Task ExecuteAsync(IClientSession session, GuriEvent e)
//     {
//         if (!_icebreakerManager.IsRegistrationActive)
//         {
//             return;
//         }
//
//         if (_icebreakerManager.RegisteredPlayers.Contains(session.PlayerEntity.Id))
//         {
//             return;
//         }
//         
//         if (session.IsMuted())
//         {
//             session.SendMsgi(MessageType.Default, Game18NConstString.IAmUnderPenalty);
//             return;
//         }
//
//         _icebreakerManager.RegisterPlayer(session.PlayerEntity.Id);
//     }
// }