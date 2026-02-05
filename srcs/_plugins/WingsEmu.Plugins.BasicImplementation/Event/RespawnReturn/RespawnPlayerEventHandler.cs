using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.RespawnReturn;

public class RespawnPlayerEventHandler : IAsyncEventProcessor<RespawnPlayerEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRespawnDefaultConfiguration _respawnDefaultConfiguration;

    public RespawnPlayerEventHandler(IRandomGenerator randomGenerator, IRespawnDefaultConfiguration respawnDefaultConfiguration, IMapManager mapManager)
    {
        _randomGenerator = randomGenerator;
        _respawnDefaultConfiguration = respawnDefaultConfiguration;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(RespawnPlayerEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMapInstance lastMap = _mapManager.GetBaseMapInstanceByMapId(session.PlayerEntity.MapId);

        if (lastMap == null)
        {
            return;
        }

        RespawnDefault getRespawn = _respawnDefaultConfiguration.GetReturn(session.PlayerEntity.HomeComponent.RespawnType);
        if (lastMap.HasMapFlag(MapFlags.ACT_4))
        {
            getRespawn = _respawnDefaultConfiguration.GetReturn(session.PlayerEntity.Faction == FactionType.Angel ? RespawnType.ACT4_ANGEL_SPAWN : RespawnType.ACT4_DEMON_SPAWN);
        }

        if (lastMap.HasMapFlag(MapFlags.ACT_5_1) || lastMap.HasMapFlag(MapFlags.ACT_5_2))
        {
            getRespawn = _respawnDefaultConfiguration.GetReturnAct5(session.PlayerEntity.HomeComponent.Act5RespawnType);
        }

        if (lastMap.HasMapFlag(MapFlags.ACT_6_1) || lastMap.HasMapFlag(MapFlags.ACT_6_2))
        {
            getRespawn = _respawnDefaultConfiguration.GetReturn(RespawnType.CYLLOAN_SPAWN);
        }

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