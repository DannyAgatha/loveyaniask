using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Arena;

public class ArenaMastersRegisterHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ARENA_OF_MASTERS_REGISTRATION };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
    }
}