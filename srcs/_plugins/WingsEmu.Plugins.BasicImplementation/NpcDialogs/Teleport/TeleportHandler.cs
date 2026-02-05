using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Teleport
{
    public class TeleportHandler : INpcDialogAsyncHandler
    {
        private readonly IGameLanguageService _langService;
        private readonly IMapManager _mapManager;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IRespawnDefaultConfiguration _respawnDefaultConfiguration;

        public TeleportHandler(
            IGameLanguageService langService,
            IMapManager mapManager,
            IRespawnDefaultConfiguration respawnDefaultConfiguration,
            IRandomGenerator randomGenerator)
        {
            _langService = langService;
            _mapManager = mapManager;
            _respawnDefaultConfiguration = respawnDefaultConfiguration;
            _randomGenerator = randomGenerator;
        }

        public NpcRunType[] NpcRunTypes => new[] { NpcRunType.WARP_TELEPORT };

        public async Task Execute(IClientSession session, NpcDialogEvent e)
        {
            INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
            if (npcEntity == null)
            {
                return;
            }

            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                return;
            }

            if (session.CantPerformActionOnAct4())
            {
                return;
            }

            if (e.Argument > 3 || e.Argument < 0)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "WarpTeleport e.Argument > 3 || e.Argument < 0");
                return;
            }

            RespawnDefault getRespawn = e.Argument switch
            {
                0 => _respawnDefaultConfiguration.GetReturn(RespawnType.NOSVILLE_SPAWN),
                1 => _respawnDefaultConfiguration.GetReturn(RespawnType.KREM_SPAWN),
                2 => _respawnDefaultConfiguration.GetReturn(RespawnType.ALVEUS_SPAWN),
                _ => null
            };

            if (getRespawn == null)
            {
                return;
            }

            IMapInstance mapInstance = _mapManager.GetBaseMapInstanceByMapId(getRespawn.MapId);
            if (mapInstance == null)
            {
                return;
            }

            int randomX = getRespawn.MapX + _randomGenerator.RandomNumber(getRespawn.Radius, -getRespawn.Radius);
            int randomY = getRespawn.MapY + _randomGenerator.RandomNumber(getRespawn.Radius, -getRespawn.Radius);

            if (mapInstance.IsBlockedZone(randomX, randomY))
            {
                randomX = getRespawn.MapX;
                randomY = getRespawn.MapY;
            }

            session.ChangeMap(getRespawn.MapId, (short)randomX, (short)randomY);
        }
    }
}
