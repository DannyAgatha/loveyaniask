using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Teleport;

public class TeleportGrenigasSquareHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.TELEPORT_GRENIGAS_SQUARE };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        /*INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null || !session.PlayerEntity.HasItem((short)ItemVnums.RUNE_PIECE))
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
        {
            return;
        }

        session.ChangeMap(2536, 26, 31);
        await session.EmitEventAsync(new InventoryRemoveItemEvent((short)ItemVnums.RUNE_PIECE));*/
    }
}