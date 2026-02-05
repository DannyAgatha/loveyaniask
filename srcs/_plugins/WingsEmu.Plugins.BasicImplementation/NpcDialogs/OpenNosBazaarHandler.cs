using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class OpenNosBazaarHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.OPEN_NOSBAZAAR };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsInExchange() || session.PlayerEntity.IsShopping || session.PlayerEntity.IsWarehouseOpen || session.PlayerEntity.IsPartnerWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseOpen)
        {
            return;
        }
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && !session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        await session.EmitEventAsync(new BazaarOpenUiEvent(false));
    }
}