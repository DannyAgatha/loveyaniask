using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game._NpcDialog;

public interface INpcDialogHandlerContainer
{
    void Register(INpcDialogAsyncHandler handler);
    void Unregister(INpcDialogAsyncHandler handler);

    void Execute(IClientSession player, NpcDialogEvent e);

    Task ExecuteAsync(IClientSession player, NpcDialogEvent e);
}