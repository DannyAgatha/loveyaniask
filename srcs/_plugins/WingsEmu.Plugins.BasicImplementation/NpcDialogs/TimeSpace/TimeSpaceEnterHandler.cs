using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.TimeSpace;

public class TimeSpaceEnterHandler : INpcDialogAsyncHandler
{
    private readonly ITimeSpaceNpcRunConfig _timeSpaceNpcRunConfig;

    public TimeSpaceEnterHandler(ITimeSpaceNpcRunConfig timeSpaceNpcRunConfig) => _timeSpaceNpcRunConfig = timeSpaceNpcRunConfig;

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ENTER_TO_TS_ID };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        if (!_timeSpaceNpcRunConfig.DoesTimeSpaceExist(e.Argument))
        {
            return;
        }

        if (session.PlayerEntity.IsInGroup())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NEED_LEAVE_GROUP), MsgMessageType.Middle);
            return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) || !session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        int? quest = _timeSpaceNpcRunConfig.GetQuestByTimeSpaceId(e.Argument);
        if (quest.HasValue)
        {
            if (!session.PlayerEntity.HasQuestWithId(quest.Value))
            {
                return;
            }
        }

        await session.EmitEventAsync(new TimeSpacePartyCreateEvent(e.Argument));
        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
    }
}