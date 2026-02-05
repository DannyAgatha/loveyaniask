using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.FamilyImpl.NpcDialogs
{
    public class OpenFamilyWarehouseHandler : INpcDialogAsyncHandler
    {
        public NpcRunType[] NpcRunTypes => new[] { NpcRunType.FAMILY_WAREHOUSE_OPEN };

        public async Task Execute(IClientSession session, NpcDialogEvent e)
        {
            await session.EmitEventAsync(new FamilyWarehouseOpenEvent());
        }
    }
}