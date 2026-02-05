using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game._NpcDialog;

public interface INpcDialogAsyncHandler
{
    NpcRunType[] NpcRunTypes { get; }

    Task Execute(IClientSession session, NpcDialogEvent e);
}