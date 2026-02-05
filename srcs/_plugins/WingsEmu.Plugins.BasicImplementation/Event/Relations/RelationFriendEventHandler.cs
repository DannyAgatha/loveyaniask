using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Relations;

namespace NosEmu.Plugins.BasicImplementations.Event.Relations;

public class RelationFriendEventHandler : IAsyncEventProcessor<RelationFriendEvent>
{
    private readonly IInvitationManager _invitationManager;
    private readonly IGameLanguageService _language;
    private readonly ISessionManager _sessionManager;

    public RelationFriendEventHandler(IGameLanguageService language, ISessionManager sessionManager, IInvitationManager invitationManager)
    {
        _sessionManager = sessionManager;
        _invitationManager = invitationManager;
        _language = language;
    }

    public async Task HandleAsync(RelationFriendEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        FInsPacketType type = e.RequestType;

        if (session.PlayerEntity.IsFriendsListFull())
        {
            session.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_FRIENDLIST_FULL, session.UserLanguage));
            return;
        }

        long characterId = e.CharacterId;

        if (session.PlayerEntity.Id == characterId)
        {
            return;
        }

        if (session.PlayerEntity.IsBlocking(characterId))
        {
            session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKING, session.UserLanguage));
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        IClientSession otherSession = _sessionManager.GetSessionByCharacterId(characterId);
        if (otherSession == null)
        {
            return;
        }
        
        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.UnderWaterShowdown)
        {
            return;
        }

        if (session.PlayerEntity.IsFriend(characterId))
        {
            session.SendInfo(_language.GetLanguageFormat(GameDialogKey.FRIEND_INFO_ALREADY_FRIEND, session.UserLanguage, otherSession.PlayerEntity.Name));
            return;
        }

        if (session.PlayerEntity.IsMarried(characterId))
        {
            session.SendInfo(_language.GetLanguageFormat(GameDialogKey.FRIEND_INFO_ALREADY_FRIEND, session.UserLanguage, otherSession.PlayerEntity.Name));
            return;
        }

        if (otherSession.PlayerEntity.FriendRequestBlocked)
        {
            session.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_REQUEST_BLOCKED, session.UserLanguage));
            return;
        }

        if (otherSession.PlayerEntity.IsFriendsListFull())
        {
            session.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_FRIENDLIST_FULL, session.UserLanguage));
            return;
        }

        if (otherSession.PlayerEntity.IsBlocking(session.PlayerEntity.Id))
        {
            session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKED, session.UserLanguage));
            return;
        }

        if (type != FInsPacketType.INVITE)
        {
            if (!_invitationManager.ContainsPendingInvitation(otherSession.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.Friend))
            {
                return;
            }

            _invitationManager.RemovePendingInvitation(otherSession.PlayerEntity.Id, session.PlayerEntity.Id, InvitationType.Friend);
        }

        switch (type)
        {
            case FInsPacketType.INVITE:
                await session.EmitEventAsync(new InvitationEvent(otherSession.PlayerEntity.Id, InvitationType.Friend));
                break;
            case FInsPacketType.ACCEPT:
                session.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_ADDED, session.UserLanguage));
                otherSession.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_ADDED, otherSession.UserLanguage));

                await session.AddRelationAsync(characterId, CharacterRelationType.Friend);
                break;
            case FInsPacketType.REFUSE:
                otherSession.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_REFUSED, otherSession.UserLanguage));
                break;
        }
    }
}