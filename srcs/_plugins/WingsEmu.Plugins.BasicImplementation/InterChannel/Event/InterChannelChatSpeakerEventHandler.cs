using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event
{
    internal class InterChannelChatSpeakerEventHandler : IAsyncEventProcessor<InterchannelChatSpeakerEvent>
    {
        private readonly IMessagePublisher<InterChannelSpeakerMessage> _messagePublisher;
        private readonly IGameLanguageService _languageService;
        private readonly IItemsManager _itemsManager;
        private readonly ICharacterAlgorithm _characterAlgorithm;

        public InterChannelChatSpeakerEventHandler(IMessagePublisher<InterChannelSpeakerMessage> messagePublisher, IGameLanguageService languageService, IItemsManager itemsManager, ICharacterAlgorithm characterAlgorithm)
        {
            _messagePublisher = messagePublisher;
            _languageService = languageService;
            _itemsManager = itemsManager;
            _characterAlgorithm = characterAlgorithm;
        }

        private string GenerateLanguageMessage(ChatSpeakerEvent e, IClientSession recv, int channelId)
        {
            IClientSession sender = e.Sender;
            SpeakerType chatSpeakerType = e.ChatSpeakerType;
            string message = e.Message;
            message = message.Trim();

            string messageHeader = $"<{_languageService.GetLanguage(GameDialogKey.SPEAKER_NAME, recv.UserLanguage)} Channel-{channelId}>";
            messageHeader += chatSpeakerType == SpeakerType.Normal_Speaker ? $" [{sender.PlayerEntity.Name}]: " : $"|[{sender.PlayerEntity.Name}]:|"; // Weird packet handling 
            message = messageHeader + message;
            if (message.Length > 120)
            {
                message = message[..120];
            }

            return message;
        }

        public async Task HandleAsync(InterchannelChatSpeakerEvent e, CancellationToken cancellation)
        {
            string message = e.Event.ChatSpeakerType is SpeakerType.Items_Speaker 
                ? e.Sender.GenerateItemSpeaker(e.Event.Item, e.Event.Message, _itemsManager, _characterAlgorithm) 
                : e.Sender.PlayerEntity.GenerateSayPacket(GenerateLanguageMessage(e.Event, e.Sender, e.ChannelId), ChatMessageColorType.LightPurple);
            
            await _messagePublisher.PublishAsync(new InterChannelSpeakerMessage
            {
                Message = message,
                ChannelId = e.ChannelId,
                UserId = e.Sender.PlayerEntity.Id
            }, cancellation);
        }
    }
}