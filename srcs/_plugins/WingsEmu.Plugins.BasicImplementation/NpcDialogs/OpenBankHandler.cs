using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class OpenBankHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.USE_BANK };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        await session.EmitEventAsync(new BankOpenEvent
        {
            NpcId = e.NpcId
        });
    }
}