// using System.Threading.Tasks;
// using WingsEmu.Game._enum;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Battle;
// using WingsEmu.Game.Buffs;
// using WingsEmu.Game.Configurations;
// using WingsEmu.Game.Entities;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Managers;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;
//
// public class GlacernonBuffersHandler : INpcDialogAsyncHandler
// {
//     private readonly IBuffFactory _buffFactory;
//     private readonly IGameLanguageService _gameLanguage;
//     private readonly IRankingManager _rankingManager;
//     private readonly IReputationConfiguration _reputationConfiguration;
//
//     public GlacernonBuffersHandler(IBuffFactory buffFactory, IGameLanguageService gameLanguage, IRankingManager rankingManager, IReputationConfiguration reputationConfiguration)
//     {
//         _buffFactory = buffFactory;
//         _gameLanguage = gameLanguage;
//         _rankingManager = rankingManager;
//         _reputationConfiguration = reputationConfiguration;
//     }
//
//     public NpcRunType[] NpcRunTypes => new[]
//     {
//         NpcRunType.GLACERNON_BUFF_1,
//         NpcRunType.GLACERNON_BUFF_2,
//         NpcRunType.GLACERNON_BUFF_3,
//     };
//     
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
//         INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
//         
//         if (npcEntity == null)
//         {
//             return;
//         }
//         
//         const int reputationLoseCost = 25_000; 
//
//         if (session.PlayerEntity.Reput < reputationLoseCost)
//         {
//             return;
//         }
//         
//         BuffVnums buffToApply;
//
//         switch (e.NpcRunType)
//         {
//             case NpcRunType.GLACERNON_BUFF_1:
//                 buffToApply = BuffVnums.GLACERNON_WARRIOR;
//                 break;
//             case NpcRunType.GLACERNON_BUFF_2:
//                 buffToApply = BuffVnums.GLACERNON_ASSASSIN;
//                 break;
//             case NpcRunType.GLACERNON_BUFF_3:
//                 buffToApply = BuffVnums.GLACERNON_KNIGHT;
//                 break;
//             default:
//                 return;
//         }
//         
//         await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff((int)buffToApply, session.PlayerEntity));
//         session.PlayerEntity.Reput -= reputationLoseCost;
//         session.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);
//         session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.INFORMATION_CHATMESSAGE_REPUT_DECREASE, session.UserLanguage, reputationLoseCost), ChatMessageColorType.Red);
//         session.RefreshStat();
//     }
// }