using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class JewelryWindowHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.JEWELRY_CELLON };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsInExchange())
        {
            return;
        }
        
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        session.SendWopenPacket((byte)WindowType.MERGE_JEWELRY);
    }
}