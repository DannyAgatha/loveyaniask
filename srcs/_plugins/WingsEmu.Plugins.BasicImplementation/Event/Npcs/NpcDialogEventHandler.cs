using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;

namespace NosEmu.Plugins.BasicImplementations.Event.Npcs;

public class NpcDialogEventHandler : IAsyncEventProcessor<NpcDialogEvent>
{
    private readonly INpcDialogHandlerContainer _npcDialogHandler;

    public NpcDialogEventHandler(INpcDialogHandlerContainer npcDialogHandler) => _npcDialogHandler = npcDialogHandler;

    public async Task HandleAsync(NpcDialogEvent e, CancellationToken cancellation)
    {
        await _npcDialogHandler.ExecuteAsync(e.Sender, e);
    }
}