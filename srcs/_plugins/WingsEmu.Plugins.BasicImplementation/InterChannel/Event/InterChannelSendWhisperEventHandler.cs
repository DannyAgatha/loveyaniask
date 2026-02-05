using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Player;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event;

public class InterChannelSendWhisperEventHandler : IAsyncEventProcessor<InterChannelSendWhisperEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelSendWhisperMessage> _messagePublisher;
    private readonly IServerManager _serverManager;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendWhisperEventHandler(ISessionManager sessionManager, IMessagePublisher<InterChannelSendWhisperMessage> messagePublisher, IGameLanguageService languageService,
        IServerManager serverManager)
    {
        _sessionManager = sessionManager;
        _messagePublisher = messagePublisher;
        _languageService = languageService;
        _serverManager = serverManager;
    }

    public async Task HandleAsync(InterChannelSendWhisperEvent e, CancellationToken cancellation)
    {
        if (e.Nickname == e.Sender.PlayerEntity.Name)
        {
            return;
        }

        IClientSession session = e.Sender;
        IClientSession localSession = _sessionManager.GetSessionByCharacterName(e.Nickname);

        if (localSession == null)
        {
            if (!_sessionManager.IsOnline(e.Nickname))
            {
                e.Sender.SendErrorChatMessage(_languageService.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_CONNECTED, e.Sender.UserLanguage));
                return;
            }

            e.Sender.SendSpeak(e.Message, SpeakType.Player);
            await _messagePublisher.PublishAsync(new InterChannelSendWhisperMessage
            {
                SenderCharacterId = e.Sender.PlayerEntity.Id,
                SenderNickname = e.Sender.PlayerEntity.Name,
                SenderChannelId = _serverManager.ChannelId,
                AuthorityType = e.Sender.PlayerEntity.Authority,
                Message = e.Message,
                ReceiverNickname = e.Nickname
            }, cancellation);

            ClusterCharacterInfo messageTarget = _sessionManager.GetOnlineCharacterByName(e.Nickname);
            e.Sender.SendChatMessage(_languageService.GetLanguageFormat(GameDialogKey.INFORMATION_CHATMESSAGE_USER_WHISPER_SENT, e.Sender.UserLanguage, e.Nickname, messageTarget.ChannelId),
                ChatMessageColorType.Red);
            await e.Sender.EmitEventAsync(new ChatGenericEvent
            {
                Message = e.Message,
                ChatType = ChatType.Whisper,
                TargetCharacterId = messageTarget.Id
            });
            return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) && localSession.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            if (session.PlayerEntity.Faction != localSession.PlayerEntity.Faction && !session.IsGameMaster() && !localSession.IsGameMaster())
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.FRIEND_TALKMESSAGE_DIFFRENT_FACTION), ChatMessageColorType.Red);
                return;
            }
        }

        e.Sender.SendSpeak(e.Message, SpeakType.Player);
        await localSession.EmitEventAsync(new InterChannelReceiveWhisperEvent(e.Sender.PlayerEntity.Id, e.Sender.PlayerEntity.Name, -1, e.Sender.PlayerEntity.Authority,
            e.Message));
        await e.Sender.EmitEventAsync(new ChatGenericEvent
        {
            Message = e.Message,
            ChatType = ChatType.Whisper,
            TargetCharacterId = localSession.PlayerEntity.Id
        });
    }
}