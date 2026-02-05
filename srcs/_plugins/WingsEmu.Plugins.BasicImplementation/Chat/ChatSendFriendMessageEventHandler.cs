using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.Relation;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace NosEmu.Plugins.BasicImplementations.Chat;

public class ChatSendFriendMessageEventHandler : IAsyncEventProcessor<ChatSendFriendMessageEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMessagePublisher<RelationSendTalkMessage> _messagePublisher;
    private readonly ISessionManager _sessionManager;

    public ChatSendFriendMessageEventHandler(ISessionManager sessionManager, IGameLanguageService gameLanguage, IMessagePublisher<RelationSendTalkMessage> messagePublisher)
    {
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(ChatSendFriendMessageEvent e, CancellationToken cancellation)
    {
        long targetId = e.TargetId;
        string message = e.Message;
        IClientSession session = e.Sender;

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (session.PlayerEntity.Id == targetId)
        {
            return;
        }

        if (message.Length > 60)
        {
            message = message.Substring(0, 60);
        }

        message = message.Trim();

        if (!session.PlayerEntity.IsFriend(targetId) && !session.PlayerEntity.IsMarried(targetId))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.FRIEND_MESSAGE_NOT_FRIEND, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        IClientSession target = _sessionManager.GetSessionByCharacterId(targetId);
        if (target == null)
        {
            if (!_sessionManager.IsOnline(targetId))
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_CONNECTED, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            await _messagePublisher.PublishAsync(new RelationSendTalkMessage
            {
                Message = message,
                TargetId = targetId,
                SenderId = session.PlayerEntity.Id
            });
            await session.EmitEventAsync(new ChatGenericEvent
            {
                Message = e.Message,
                ChatType = ChatType.FriendChat,
                TargetCharacterId = targetId
            });
            return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) && target.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            if (session.PlayerEntity.Faction != target.PlayerEntity.Faction)
            {
                message = _gameLanguage.GetLanguage(GameDialogKey.FRIEND_TALKMESSAGE_DIFFRENT_FACTION, session.UserLanguage);
                session.SendFriendMessage(targetId, message);
                return;
            }
        }

        await session.EmitEventAsync(new ChatGenericEvent
        {
            Message = e.Message,
            ChatType = ChatType.FriendChat,
            TargetCharacterId = targetId
        });
        target.SendFriendMessage(session.PlayerEntity.Id, message);
    }
}