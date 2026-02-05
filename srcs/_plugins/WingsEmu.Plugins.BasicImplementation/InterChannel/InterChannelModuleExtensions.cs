using Microsoft.Extensions.DependencyInjection;
using NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;
using PhoenixLib.ServiceBus.Extensions;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel;

public static class InterChannelModuleExtensions
{
    public static void AddInterChannelModule(this IServiceCollection services)
    {
        services.AddMessagePublisher<InterChannelSendChatMsgByCharIdMessage>();
        services.AddMessageSubscriber<InterChannelSendChatMsgByCharIdMessage, InterChannelSendChatMsgByCharIdMessageConsumer>();

        services.AddMessagePublisher<InterChannelSendChatMsgByNicknameMessage>();
        services.AddMessageSubscriber<InterChannelSendChatMsgByNicknameMessage, InterChannelSendChatMsgByNicknameMessageConsumer>();

        services.AddMessagePublisher<InterChannelSendWhisperMessage>();
        services.AddMessageSubscriber<InterChannelSendWhisperMessage, InterChannelSendWhisperMessageConsumer>();

        services.AddMessagePublisher<InterChannelSendInfoByCharIdMessage>();
        services.AddMessageSubscriber<InterChannelSendInfoByCharIdMessage, InterChannelSendInfoByCharIdMessageConsumer>();

        services.AddMessagePublisher<InterChannelSendInfoByNicknameMessage>();
        services.AddMessageSubscriber<InterChannelSendInfoByNicknameMessage, InterChannelSendInfoByNicknameMessageConsumer>();

        services.AddMessagePublisher<InterChannelChatMessageBroadcastMessage>();
        services.AddMessageSubscriber<InterChannelChatMessageBroadcastMessage, InterChannelChatMessageBroadcastMessageConsumer>();
        
        services.AddMessagePublisher<InterChannelSendMsgBroadcastMessage>();
        services.AddMessageSubscriber<InterChannelSendMsgBroadcastMessage, InterChannelSendMsgBroadcastMessageConsumer>();

        services.AddMessagePublisher<ChatShoutAdminMessage>();
        services.AddMessageSubscriber<ChatShoutAdminMessage, ChatShoutAdminMessageConsumer>();
        
        services.AddMessagePublisher<LandOfLifeMessage>();
        services.AddMessageSubscriber<LandOfLifeMessage, LandOfLifeMessageConsumer>();

        services.AddMessagePublisher<BazaarNotificationMessage>();
        services.AddMessageSubscriber<BazaarNotificationMessage, BazaarNotificationMessageConsumer>();
        
        services.AddMessagePublisher<InterChannelSpeakerMessage>();
        services.AddMessageSubscriber<InterChannelSpeakerMessage, InterChannelSpeakerMessageConsumer>();
        
        services.AddMessagePublisher<GlacernonPercentageMessage>();
        services.AddMessageSubscriber<GlacernonPercentageMessage, GlacernonPercentageMessageConsumer>();
    }
}