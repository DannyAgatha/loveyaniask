using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidPartyDisbandEventHandler : IAsyncEventProcessor<RaidPartyDisbandEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IAct4CaligorManager _act4CaligorManager;


    public RaidPartyDisbandEventHandler(IGameLanguageService gameLanguage, IRaidManager raidManager, IAsyncEventPipeline eventPipeline, IAct4CaligorManager act4CaligorManager)
    {
        _gameLanguage = gameLanguage;
        _eventPipeline = eventPipeline;
        _act4CaligorManager = act4CaligorManager;
    }

    public async Task HandleAsync(RaidPartyDisbandEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        RaidParty raid = session.PlayerEntity.Raid;

        if (raid?.Members == null)
        {
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }

        if (session.PlayerEntity.Raid.Finished && e.IsByRdPacket)
        {
            return;
        }

        if (session.PlayerEntity.HasRaidStarted && session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id) && raid.Members.Count > 1)
        {
            await session.EmitEventAsync(new RaidPartyLeaveEvent(false));
            return;
        }

        foreach (IClientSession member in raid.Members)
        {
            await RemoveFromRaid(member, raid);
        }

        await _eventPipeline.ProcessEventAsync(new RaidInstanceFinishEvent(raid, RaidFinishType.Disbanded), cancellation);
        await session.EmitEventAsync(new RaidAbandonedEvent { RaidId = raid.Id });
    }

    private async Task RemoveFromRaid(IClientSession session, RaidParty raidParty)
    {
        await RaidPartyLeaveEventHandler.InternalLeave(session);

        if (raidParty.Started)
        {
            if (!session.PlayerEntity.IsAlive())
            {
                session.EmitEvent(new RevivalReviveEvent());
            }

            if (raidParty.Type == RaidType.Fernon)
            {
                if (_act4CaligorManager.FernonMapsActive)
                {
                    short x = session.PlayerEntity.PositionBeforeFernonRaidEnter.X;
                    short y = session.PlayerEntity.PositionBeforeFernonRaidEnter.Y;
                    session.ChangeMap(_act4CaligorManager.FernonMap, x, y);
                }
                else
                {
                    session.ChangeMap(153, 93, 93);
                }
            }
            else
            {
                session.ChangeToLastBaseMap();
            }

            await session.EmitEventAsync(new RaidPartyLeaveEvent(false));
        }

        session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_MESSAGE_DISOLVED, session.UserLanguage), ChatMessageColorType.Red);
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_MESSAGE_DISOLVED, session.UserLanguage), MsgMessageType.Middle);
    }
}