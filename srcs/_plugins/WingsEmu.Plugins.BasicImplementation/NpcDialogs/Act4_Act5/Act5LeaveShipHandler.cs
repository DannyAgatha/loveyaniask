using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Ship.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Act4_Act5;

public class Act5LeaveShipHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ACT5_LEAVE_SHIP };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }
        
        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.NormalInstance)
        {
            return;
        }

        await session.EmitEventAsync(new ShipLeaveEvent());
    }
}