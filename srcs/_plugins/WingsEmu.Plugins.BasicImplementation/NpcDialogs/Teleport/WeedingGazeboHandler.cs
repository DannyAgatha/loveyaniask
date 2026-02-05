using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Teleport;

public class WeedingGazeboHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly IMapManager _mapManager;

    public WeedingGazeboHandler(IGameLanguageService langService, IMapManager mapManager)
    {
        _mapManager = mapManager;
        _langService = langService;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.WEDDING_GAZEBO };

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

        int baseToRemove = 5000 * e.Argument * 2;
        short toRemove = session.PlayerEntity.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.DECREASE_SHIP_TP_COST).Item1 ?? 0;
        int amountToRemove = (int)(baseToRemove * (toRemove * 0.01));
        baseToRemove -= amountToRemove;

        if (session.PlayerEntity.Gold < baseToRemove)
        {
            session.SendChatMessage(_langService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        session.PlayerEntity.Gold -= baseToRemove;
        session.RefreshGold();
        session.ChangeMap(2586, 35, 52);
    }
}