// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Plugin.LandOfDeath;
// using WingsAPI.Packets.Enums.Chat;
// using WingsAPI.Packets.Enums.LandOfDeath;
// using WingsEmu.DTOs.Maps;
// using WingsEmu.Game._i18n;
// using WingsEmu.Game._NpcDialog;
// using WingsEmu.Game._NpcDialog.Event;
// using WingsEmu.Game.Configurations;
// using WingsEmu.Game.Extensions;
// using WingsEmu.Game.Groups;
// using WingsEmu.Game.LandOfDeath;
// using WingsEmu.Game.Networking;
// using WingsEmu.Packets.Enums;
// using WingsEmu.Packets.Enums.Chat;
//
// namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.LandOfDeath;
// public class JoinLodHandler : INpcDialogAsyncHandler
// {
//     public NpcRunType[] NpcRunTypes =>
//     [
//         NpcRunType.ENTER_THE_LAND_OF_DEATH,
//     ];
//
//     private readonly ILandOfDeathManager _landOfDeathManager;
//     private readonly ILandOfDeathFactory _landOfDeathFactory;
//     private readonly LandOfDeathConfiguration _landOfDeathConfiguration;
//
//     public JoinLodHandler(
//         ILandOfDeathManager landOfDeathManager,
//         ILandOfDeathFactory landOfDeathFactory,
//         LandOfDeathConfiguration landOfDeathConfiguration)
//     {
//         _landOfDeathManager = landOfDeathManager;
//         _landOfDeathFactory = landOfDeathFactory;
//         _landOfDeathConfiguration = landOfDeathConfiguration;
//     }
//
//     public async Task Execute(IClientSession session, NpcDialogEvent e)
//     {
//         if (session.CurrentMapInstance == null)
//         {
//             return;
//         }
//
//         if (!_landOfDeathManager.IsActive)
//         {
//             session.SendMsg(GameDialogKey.LAND_OF_DEATH_MESSAGE_CLOSED, MsgMessageType.Middle);
//             return;
//         }
//
//         if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
//         {
//             return;
//         }
//
//         if (session.PlayerEntity.Level < _landOfDeathConfiguration.MinLevel)
//         {
//             session.SendMsg(GameDialogKey.LAND_OF_DEATH_MESSAGE_LOW_LEVEL, MsgMessageType.Middle);
//             return;
//         }
//
//         if (e.Confirmation is null or 0)
//         {
//             session.SendQnaiPacket(
//                 $"n_run {(int)e.NpcRunType} 2 {e.NpcId} 1",
//                 Game18NConstString.AskJoinLandOfDeath
//             );
//             return;
//         }
//
//         LandOfDeathInstance instance = null;
//
//         switch (e.NpcRunType)
//         {
//             case NpcRunType.ENTER_THE_LAND_OF_DEATH:
//                 instance = _landOfDeathManager.GetLandOfDeathInstanceByFamilyId(session.PlayerEntity.Family.Id, mode)
//                            ?? _landOfDeathFactory.CreateFamilyLandOfDeath(session.PlayerEntity.Family.Id, mode);
//                 break;
//         }
//
//         if (instance?.MapInstance == null)
//         {
//             return;
//         }
//
//         session.ChangeMap(
//             instance.MapInstance,
//             _landOfDeathConfiguration.MapSpawnPositionX,
//             _landOfDeathConfiguration.MapSpawnPositionY
//         );
//
//         instance.LastPlayerId = session.PlayerEntity.Id;
//         session.SendTsClockPacket(_landOfDeathManager.End - DateTime.UtcNow, true);
//     }
// }