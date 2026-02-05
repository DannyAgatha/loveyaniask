using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Consumers;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums.Chat;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace Plugin.FamilyImpl
{
    public class FamilyChatMessageEventHandler : IAsyncEventProcessor<FamilyChatMessageEvent>
    {
        private readonly FamilyChatMessageConsumer _familyChatMessageConsumer;
        private readonly IMessagePublisher<FamilyChatMessage> _messagePublisher;
        private readonly IServerManager _serverManager;

        public FamilyChatMessageEventHandler(IMessagePublisher<FamilyChatMessage> messagePublisher, IServerManager serverManager, FamilyChatMessageConsumer familyChatMessageConsumer)
        {
            _messagePublisher = messagePublisher;
            _serverManager = serverManager;
            _familyChatMessageConsumer = familyChatMessageConsumer;
        }

        public async Task HandleAsync(FamilyChatMessageEvent e, CancellationToken cancellation)
        {
            IFamily family = e.Sender.PlayerEntity.Family;
            if (family == null)
            {
                return;
            }

            if (family.Members.Count <= 1)
            {
                e.Sender.SendChatMessageNoId($"[{e.Sender.PlayerEntity.Name}]:{e.Message}", ChatMessageColorType.Blue);
                e.Sender.SendSpeak(e.Message, SpeakType.Family);
                return;
            }

            e.Sender.BroadcastSpeak(e.Message, SpeakType.Family, new FamilyBroadcast(family.Id));

            var messageToPublish = new FamilyChatMessage
            {
                SenderFamilyId = family.Id,
                SenderChannelId = _serverManager.ChannelId,
                SenderNickname = e.Sender.PlayerEntity.Name,
                Message = e.Message
            };

            await e.Sender.EmitEventAsync(new ChatGenericEvent
            {
                Message = e.Message,
                ChatType = ChatType.FamilyChat
            });

            await _messagePublisher.PublishAsync(messageToPublish, cancellation);

            await _familyChatMessageConsumer.HandleAsync(messageToPublish, cancellation);
        }
    }
}