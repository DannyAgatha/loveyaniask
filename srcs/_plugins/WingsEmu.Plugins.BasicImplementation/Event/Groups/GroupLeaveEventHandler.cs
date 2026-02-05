using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Groups;

public class LeaveGroupEventHandler : IAsyncEventProcessor<LeaveGroupEvent>
{
    private readonly IGroupManager _groupManager;

    public LeaveGroupEventHandler(IGroupManager groupManager) => _groupManager = groupManager;

    public async Task HandleAsync(LeaveGroupEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        PlayerGroup playerGroup = session.PlayerEntity.GetGroup();
        IReadOnlyList<IPlayerEntity> otherMembers = playerGroup.Members;

        bool closed = false;
        bool isLeader = session.PlayerEntity.IsLeaderOfGroup(session.PlayerEntity.Id);
        var toRemove = new List<(PlayerGroup, IPlayerEntity)>();

        foreach (IPlayerEntity member in otherMembers)
        {
            if (member.Id == session.PlayerEntity.Id)
            {
                continue;
            }

            if (otherMembers.Count - 1 > 1)
            {
                break;
            }

            toRemove.Add((playerGroup, member));
            closed = true;
        }

        _groupManager.RemoveMemberGroup(playerGroup, session.PlayerEntity);
        session.SendMsg(session.GetLanguage(GameDialogKey.GROUP_INFO_LEFT), MsgMessageType.Middle);
        await session.EmitEventAsync(new GroupWeedingEvent
        {
            RemoveBuff = true
        });

        if (session.CurrentMapInstance is { MapInstanceType: MapInstanceType.ArenaInstance })
        {
            session.SendArenaStatistics(false, playerGroup);
        }

        foreach ((PlayerGroup group, IPlayerEntity character) in toRemove)
        {
            _groupManager.RemoveMemberGroup(group, character);
            character.Session.SendMsg(character.Session.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_CLOSED), MsgMessageType.Middle);
            if (character.MapInstance is { MapInstanceType: MapInstanceType.ArenaInstance })
            {
                character.Session.SendArenaStatistics(false);
            }
        }

        if (closed)
        {
            return;
        }

        if (!isLeader)
        {
            return;
        }

        var members = otherMembers.ToList();
        if (!members.Any())
        {
            return;
        }

        if (members.Count <= 1)
        {
            foreach (IPlayerEntity member in members)
            {
                _groupManager.RemoveMemberGroup(member.GetGroup(), member);
                member.Session.SendMsg(member.Session.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_CLOSED), MsgMessageType.Middle);
                if (member.MapInstance is { MapInstanceType: MapInstanceType.ArenaInstance })
                {
                    member.Session.SendArenaStatistics(false);
                }
            }

            return;
        }

        IClientSession newLeader = members.ElementAt(1).Session;
        if (newLeader == null)
        {
            return;
        }

        _groupManager.ChangeLeader(playerGroup, newLeader.PlayerEntity.Id);
        newLeader.SendInfo(newLeader.GetLanguage(GameDialogKey.GROUP_INFO_NEW_LEADER));
        if (newLeader.CurrentMapInstance is { MapInstanceType: MapInstanceType.ArenaInstance })
        {
            newLeader.SendArenaStatistics(false, playerGroup);
        }
    }
}