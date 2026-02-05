using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Arena;

public class ArenaSpectatorHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ARENA_OF_MASTERS_SPECTATOR_CHOICE };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        /*INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }
        
        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.ArenaInstance)
        {
            return;
        }
        
        session.SendSpectatorWindow();*/
    }
}