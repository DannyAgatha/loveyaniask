using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
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
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Teleport;

public class WarpTeleportAct5Handler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly IMapManager _mapManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRespawnDefaultConfiguration _respawnDefaultConfiguration;

    public WarpTeleportAct5Handler(IGameLanguageService langService, IMapManager mapManager, IRespawnDefaultConfiguration respawnDefaultConfiguration, IRandomGenerator randomGenerator)
    {
        _langService = langService;
        _mapManager = mapManager;
        _respawnDefaultConfiguration = respawnDefaultConfiguration;
        _randomGenerator = randomGenerator;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.WARP_TELEPORT_ACT5 };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_5_1) && !session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_5_2))
        {
            return;
        }

        int baseToRemove = 5000 * e.Argument;
        short toRemove = session.PlayerEntity.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.DECREASE_SHIP_TP_COST).Item1 ?? 0;
        int amountToRemove = (int)(baseToRemove * (toRemove * 0.01));
        baseToRemove -= amountToRemove;

        if (session.PlayerEntity.Gold < baseToRemove)
        {
            session.SendChatMessage(_langService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (e.Argument is > 3 or < 0)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "WarpTeleportAct5 e.Argument > 3 || e.Argument < 0");
            return;
        }

        RespawnDefault getRespawn = e.Argument switch
        {
            0 => _respawnDefaultConfiguration.GetReturnAct5(Act5RespawnType.MORTAZ_DESERT_PORT),
            1 => _respawnDefaultConfiguration.GetReturnAct5(Act5RespawnType.AKAMUR_CAMP),
            2 => _respawnDefaultConfiguration.GetReturnAct5(Act5RespawnType.DESERT_EAGLE_CITY),
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
        session.PlayerEntity.Gold -= baseToRemove;
        session.RefreshGold();
    }
}