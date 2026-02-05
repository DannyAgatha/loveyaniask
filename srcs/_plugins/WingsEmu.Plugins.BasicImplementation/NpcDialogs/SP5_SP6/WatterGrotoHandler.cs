using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.SP5_SP6;

public class WatterGrotoHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ENTER_TO_WATTER_GROTO };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        /*INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
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

        session.ChangeMap(2587, 35, 14);*/
    }
}