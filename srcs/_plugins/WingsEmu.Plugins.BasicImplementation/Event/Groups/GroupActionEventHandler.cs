using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Groups;

public class GroupActionEventHandler : IAsyncEventProcessor<GroupActionEvent>
{
    private readonly IGroupFactory _groupFactory;
    private readonly IInvitationManager _invitation;
    private readonly IGameLanguageService _language;
    private readonly ISessionManager _sessionManager;

    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public GroupActionEventHandler(IGameLanguageService language, ISessionManager sessionManager, IInvitationManager invitation, IGroupFactory groupFactory)
    {
        _sessionManager = sessionManager;
        _invitation = invitation;
        _groupFactory = groupFactory;
        _language = language;
    }

    public async Task HandleAsync(GroupActionEvent e, CancellationToken cancellation)
    {
        await _semaphoreSlim.WaitAsync(cancellation);
        try
        {
            IClientSession session = e.Sender;
            if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
            {
                return;
            }
            
            if (session.PlayerEntity.MapInstance.IsAct6PvpInstance)
            {
                return;
            }
            
            IClientSession sender;
            switch (e.RequestType)
            {
                case GroupRequestType.Requested:
                case GroupRequestType.Invited:
                    if (session.PlayerEntity.IsInGroup() && session.PlayerEntity.IsGroupFull())
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_FULL, session.UserLanguage));
                        return;
                    }

                    if (session.PlayerEntity.Id == e.CharacterId)
                    {
                        return;
                    }

                    if (session.PlayerEntity.IsInRaidParty)
                    {
                        return;
                    }
                    
                    if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.UnderWaterShowdown)
                    {
                        return;
                    }

                    if (session.PlayerEntity.HasRaidStarted)
                    {
                        return;
                    }

                    IClientSession target = _sessionManager.GetSessionByCharacterId(e.CharacterId);
                    if (target == null)
                    {
                        return;
                    }

                    if (session.PlayerEntity.IsBlocking(e.CharacterId))
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKING, session.UserLanguage));
                        return;
                    }

                    if (target.PlayerEntity.IsBlocking(session.PlayerEntity.Id))
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKED, session.UserLanguage));
                        return;
                    }

                    if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                    {
                        if (session.PlayerEntity.IsSeal)
                        {
                            return;
                        }

                        if (target.PlayerEntity.Faction != session.PlayerEntity.Faction)
                        {
                            return;
                        }
                    }

                    if (target.PlayerEntity.GroupRequestBlocked)
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_BLOCKED, session.UserLanguage));
                        return;
                    }

                    if (session.PlayerEntity.IsInGroup() && target.PlayerEntity.IsInGroup())
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_ALREADY_IN_GROUP, session.UserLanguage));
                        return;
                    }

                    if (target.PlayerEntity.IsInRaidParty)
                    {
                        return;
                    }

                    if (target.PlayerEntity.HasRaidStarted)
                    {
                        return;
                    }

                    if (_invitation.ContainsPendingInvitation(session.PlayerEntity.Id, target.PlayerEntity.Id, InvitationType.Group))
                    {
                        _invitation.RemovePendingInvitation(session.PlayerEntity.Id, target.PlayerEntity.Id, InvitationType.Group);
                    }

                    await session.EmitEventAsync(new InvitationEvent(target.PlayerEntity.Id, InvitationType.Group));
                    session.SendInfo(_language.GetLanguageFormat(GameDialogKey.GROUP_INFO_REQUEST, session.UserLanguage, target.PlayerEntity.Name, target.PlayerEntity.Level, target.PlayerEntity.HeroLevel));
                    break;
                case GroupRequestType.Accepted:
                    if (session.PlayerEntity.Id == e.CharacterId)
                    {
                        return;
                    }

                    sender = _sessionManager.GetSessionByCharacterId(e.CharacterId);
                    if (sender == null)
                    {
                        return;
                    }

                    if (!_invitation.ContainsPendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.Group))
                    {
                        return;
                    }

                    _invitation.RemovePendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.Group);

                    if (session.PlayerEntity.IsBlocking(e.CharacterId))
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKING, session.UserLanguage));
                        return;
                    }

                    if (sender.PlayerEntity.IsBlocking(session.PlayerEntity.Id))
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKED, session.UserLanguage));
                        return;
                    }

                    if (session.PlayerEntity.IsInGroup() && sender.PlayerEntity.IsInGroup())
                    {
                        session.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_ALREADY_IN_GROUP, session.UserLanguage));
                        return;
                    }

                    if (sender.PlayerEntity.IsInRaidParty)
                    {
                        return;
                    }

                    if (sender.PlayerEntity.HasRaidStarted)
                    {
                        return;
                    }

                    if (session.PlayerEntity.IsInRaidParty)
                    {
                        return;
                    }

                    if (session.PlayerEntity.HasRaidStarted)
                    {
                        return;
                    }

                    if (session.PlayerEntity.IsInGroup())
                    {
                        if (session.PlayerEntity.IsGroupFull())
                        {
                            session.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_FULL, session.UserLanguage));
                            return;
                        }

                        PlayerGroup grp = session.PlayerEntity.GetGroup();
                        await sender.EmitEventAsync(new JoinToGroupEvent(grp));
                        await session.EmitEventAsync(new GroupAddMemberEvent(sender.PlayerEntity));
                        sender.SendInfo(session.GetLanguageFormat(GameDialogKey.GROUP_INFO_JOIN, session.PlayerEntity.Name));

                        foreach (IPlayerEntity player in grp.Members)
                        {
                            if (player.MapInstance is not { MapInstanceType: MapInstanceType.ArenaInstance })
                            {
                                continue;
                            }

                            player.Session.SendArenaStatistics(false, grp);
                        }

                        return;
                    }

                    if (!sender.PlayerEntity.IsInGroup())
                    {
                        var members = new List<IPlayerEntity> { sender.PlayerEntity, session.PlayerEntity };
                        PlayerGroup newPlayerGroup = _groupFactory.CreateGroup(3, members, sender.PlayerEntity.Id);
                        await sender.EmitEventAsync(new JoinToGroupEvent(newPlayerGroup));
                        await session.EmitEventAsync(new JoinToGroupEvent(newPlayerGroup));

                        sender.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_ADMIN, sender.UserLanguage));
                        session.SendInfo(session.GetLanguageFormat(GameDialogKey.GROUP_INFO_JOIN, sender.PlayerEntity.Name));

                        foreach (IPlayerEntity player in members)
                        {
                            if (player.MapInstance is not { MapInstanceType: MapInstanceType.ArenaInstance })
                            {
                                continue;
                            }

                            player.Session.SendArenaStatistics(false, newPlayerGroup);
                        }

                        return;
                    }

                    if (sender.PlayerEntity.IsGroupFull())
                    {
                        session.SendInfo(session.GetLanguage(GameDialogKey.GROUP_INFO_FULL));
                        return;
                    }

                    PlayerGroup getPlayerGroup = sender.PlayerEntity.GetGroup();
                    await session.EmitEventAsync(new JoinToGroupEvent(getPlayerGroup));
                    await sender.EmitEventAsync(new GroupAddMemberEvent(session.PlayerEntity));
                    session.SendInfo(session.GetLanguageFormat(GameDialogKey.GROUP_INFO_JOIN, sender.PlayerEntity.Name));

                    foreach (IPlayerEntity player in getPlayerGroup.Members)
                    {
                        if (player.MapInstance is not { MapInstanceType: MapInstanceType.ArenaInstance })
                        {
                            continue;
                        }

                        player.Session.SendArenaStatistics(false, getPlayerGroup);
                    }

                    break;
                case GroupRequestType.Declined:
                    if (session.PlayerEntity.IsInGroup())
                    {
                        return;
                    }

                    if (session.PlayerEntity.Id == e.CharacterId)
                    {
                        return;
                    }

                    sender = _sessionManager.GetSessionByCharacterId(e.CharacterId);
                    if (sender == null)
                    {
                        return;
                    }

                    if (!_invitation.ContainsPendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.Group))
                    {
                        return;
                    }

                    sender.SendChatMessage(string.Format(
                        _language.GetLanguage(GameDialogKey.GROUP_CHATMESSAGE_REQUEST_REFUSED, sender.UserLanguage), session.PlayerEntity.Name), ChatMessageColorType.Yellow);

                    _invitation.RemovePendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.Group);
                    break;
                case GroupRequestType.Sharing:

                    if (!session.PlayerEntity.IsInGroup())
                    {
                        return;
                    }

                    if (session.PlayerEntity.HomeComponent.Return == null || session.PlayerEntity.HomeComponent.Return.MapId == 0)
                    {
                        return;
                    }

                    foreach (IPlayerEntity member in session.PlayerEntity.GetGroup().Members)
                    {
                        if (member.Id == session.PlayerEntity.Id)
                        {
                            continue;
                        }

                        await session.EmitEventAsync(new InvitationEvent(member.Id, InvitationType.GroupPointShare));
                    }

                    session.SendInfo(_language.GetLanguage(GameDialogKey.GROUP_INFO_SHARE_POINT_TO_MEMBERS, session.UserLanguage));
                    break;
                case GroupRequestType.AcceptedShare:

                    if (!session.PlayerEntity.IsInGroup())
                    {
                        return;
                    }

                    sender = _sessionManager.GetSessionByCharacterId(e.CharacterId);
                    if (sender == null)
                    {
                        return;
                    }

                    if (!sender.PlayerEntity.IsInGroup())
                    {
                        return;
                    }

                    if (session.PlayerEntity.GetGroupId() != sender.PlayerEntity.GetGroupId())
                    {
                        return;
                    }

                    if (!_invitation.ContainsPendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.GroupPointShare))
                    {
                        return;
                    }

                    await session.EmitEventAsync(new ReturnChangeEvent
                    {
                        MapId = sender.PlayerEntity.HomeComponent.Return.MapId,
                        MapX = sender.PlayerEntity.HomeComponent.Return.MapX,
                        MapY = sender.PlayerEntity.HomeComponent.Return.MapY,
                        IsByGroup = true
                    });

                    _invitation.RemovePendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.GroupPointShare);
                    sender.SendMsg(_language.GetLanguageFormat(GameDialogKey.GROUP_SHOUTMESSAGE_SHARE_POINT_ACCEPTED, sender.UserLanguage, session.PlayerEntity.Name), MsgMessageType.Middle);
                    session.SendMsg(_language.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_SHARE_POINT_YOU_ACCEPTED, session.UserLanguage), MsgMessageType.Middle);
                    break;
                case GroupRequestType.DeclinedShare:

                    if (!session.PlayerEntity.IsInGroup())
                    {
                        return;
                    }

                    sender = _sessionManager.GetSessionByCharacterId(e.CharacterId);
                    if (sender == null)
                    {
                        return;
                    }

                    if (!sender.PlayerEntity.IsInGroup())
                    {
                        return;
                    }

                    if (session.PlayerEntity.GetGroupId() != sender.PlayerEntity.GetGroupId())
                    {
                        return;
                    }

                    if (!_invitation.ContainsPendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.GroupPointShare))
                    {
                        return;
                    }

                    _invitation.RemovePendingInvitation(sender.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.GroupPointShare);

                    sender.SendMsg(_language.GetLanguageFormat(GameDialogKey.GROUP_SHOUTMESSAGE_SHARE_POINT_DECLINED, sender.UserLanguage, session.PlayerEntity.Name), MsgMessageType.Middle);
                    session.SendMsg(_language.GetLanguage(GameDialogKey.GROUP_SHOUTMESSAGE_SHARE_POINT_YOU_DECLINED, session.UserLanguage), MsgMessageType.Middle);
                    break;
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}