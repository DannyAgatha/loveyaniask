using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using WingsEmu.Game._enum;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Teleport;

public class TeleportSpRockHandler : INpcDialogAsyncHandler
{
    private readonly IMapManager _mapManager;

    private readonly HashSet<int> _spMapIds = new()
    {
        (int)MapIds.SP_STONE_PAJAMA,
        (int)MapIds.SP_STONE_1,
        (int)MapIds.SP_STONE_2,
        (int)MapIds.SP_STONE_3,
        (int)MapIds.SP_STONE_4
    };

    public TeleportSpRockHandler(IMapManager mapManager) => _mapManager = mapManager;

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.QUEST_TELEPORT_TO_SP_MAP };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        IMapInstance mapInstance = _mapManager.GetBaseMapInstanceByMapId(e.Argument);
        if (mapInstance == null)
        {
            Log.Debug($"There was not a map found for VNum {e.Argument.ToString()}");
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        if (!_spMapIds.Contains(mapInstance.MapId))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to teleport on different map than SP Stone");
            return;
        }

        await _mapManager.TeleportOnRandomPlaceInMapAsync(session, mapInstance);
    }
}