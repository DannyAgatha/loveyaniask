using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class OpenWindowHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.OPEN_WINDOW };
    public async Task Execute(IClientSession session, NpcDialogEvent e) => session.SendWopenPacket((byte)e.Argument, e.Confirmation ??= 0);
}