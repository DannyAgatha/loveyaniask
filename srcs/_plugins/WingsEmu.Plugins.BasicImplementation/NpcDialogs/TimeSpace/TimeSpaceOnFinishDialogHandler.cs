using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.TimeSpace;

public class TimeSpaceOnFinishDialogHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.TIMESPACE_ON_FINISH_DIALOG };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance?.PreFinishDialog == null)
        {
            return;
        }

        IPortalEntity portal = session.CurrentMapInstance.Portals.FirstOrDefault(s => s.Type == PortalType.TSEnd &&
            session.PlayerEntity.PositionY >= s.PositionY - 1 &&
            session.PlayerEntity.PositionY <= s.PositionY + 1 &&
            session.PlayerEntity.PositionX >= s.PositionX - 1 &&
            session.PlayerEntity.PositionX <= s.PositionX + 1);

        timeSpace.Instance.PreFinishDialog = null;

        if (timeSpace.Instance.PreFinishDialogIsObjective)
        {
            timeSpace.Instance.PreFinishDialogIsObjective = false;
            timeSpace.Instance.TimeSpaceObjective.ConversationsHad++;
            await session.EmitEventAsync(new TimeSpaceRefreshObjectiveProgressEvent());
        }

        await session.EmitEventAsync(new TimeSpaceCheckObjectivesEvent
        {
            TimeSpaceParty = timeSpace,
            PlayerEnteredToEndPortal = portal != null
        });
    }
}