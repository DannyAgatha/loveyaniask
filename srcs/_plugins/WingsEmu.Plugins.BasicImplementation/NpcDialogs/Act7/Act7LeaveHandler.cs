using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Act7;

public class Act7LeaveHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => [NpcRunType.MORITIUS_CROSSING_LEAVE];

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        session.ChangeMap((int)MapIds.ALVEUS, 52, 41);
    }
}