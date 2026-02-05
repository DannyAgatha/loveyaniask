using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums.Chat;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace NosEmu.Plugins.BasicImplementations.Chat;

public class ChatSpeakerEventHandler : IAsyncEventProcessor<ChatSpeakerEvent>
{
    private readonly ICharacterAlgorithm _algorithm;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public ChatSpeakerEventHandler(IGameLanguageService languageService, ISessionManager sessionManager, IItemsManager itemsManager, ICharacterAlgorithm algorithm)
    {
        _languageService = languageService;
        _sessionManager = sessionManager;
        _itemsManager = itemsManager;
        _algorithm = algorithm;
    }

    public async Task HandleAsync(ChatSpeakerEvent e, CancellationToken cancellation)
    {
        IClientSession sender = e.Sender;
        GameItemInstance item = e.Item;
        SpeakerType chatSpeakerType = e.ChatSpeakerType;

        switch (chatSpeakerType)
        {
            case SpeakerType.Normal_Speaker:
                if (sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    _sessionManager.Broadcast(s => s.PlayerEntity.GenerateSayPacket(GenerateLanguageMessage(e, s), ChatMessageColorType.LightPurple),
                        new SpeakerHeroBroadcast(), new FactionBroadcast(sender.PlayerEntity.Faction), new ExpectBlockedPlayerBroadcast(sender.PlayerEntity.Id));
                }
                else
                {
                    _sessionManager.Broadcast(s => s.PlayerEntity.GenerateSayPacket(GenerateLanguageMessage(e, s), ChatMessageColorType.LightPurple),
                        new SpeakerHeroBroadcast(), new ExpectBlockedPlayerBroadcast(sender.PlayerEntity.Id));
                }

                break;
            case SpeakerType.Items_Speaker:

                string speakerName = sender.GenerateItemSpeaker(item, e.Message, _itemsManager, _algorithm);

                if (sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    _sessionManager.Broadcast(s => speakerName, new SpeakerHeroBroadcast(),
                        new FactionBroadcast(sender.PlayerEntity.Faction), new ExpectBlockedPlayerBroadcast(sender.PlayerEntity.Id));
                }
                else
                {
                    _sessionManager.Broadcast(s => speakerName,
                        new SpeakerHeroBroadcast(), new ExpectBlockedPlayerBroadcast(sender.PlayerEntity.Id));
                }

                break;
            default:
                return;
        }

        await sender.EmitEventAsync(new InterchannelChatSpeakerEvent
        {
            Event = e,
            Sender = sender,
            ChannelId = StaticServerManager.Instance.ChannelId
        });

        await sender.EmitEventAsync(new ChatGenericEvent
        {
            Message = (e.Message.Length > 120 ? e.Message.Substring(0, 120) : e.Message).Trim(),
            ChatType = ChatType.Shout
        });
    }

    private string GenerateLanguageMessage(ChatSpeakerEvent e, IClientSession recv)
    {
        IClientSession sender = e.Sender;
        SpeakerType chatSpeakerType = e.ChatSpeakerType;
        string message = e.Message;
        message = message.Trim();

        string messageHeader = $"<{_languageService.GetLanguage(GameDialogKey.SPEAKER_NAME, recv.UserLanguage)}>";
        messageHeader += chatSpeakerType == SpeakerType.Normal_Speaker ? $" [{sender.PlayerEntity.Name}]: " : $"|[{sender.PlayerEntity.Name}]:|"; // Weird packet handling 
        message = messageHeader + message;
        if (message.Length > 120)
        {
            message = message[..120];
        }

        return message;
    }
}