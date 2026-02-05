// NosEmu
// 


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Configuration;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.FamilyImpl.Achievements;
using Plugin.FamilyImpl.Consumers;
using Plugin.FamilyImpl.Logs;
using Plugin.FamilyImpl.Messages;
using Plugin.FamilyImpl.RecurrentJob;
using WingsEmu.Communication.gRPC.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;

namespace Plugin.FamilyImpl
{
    public static class FamiliesModuleExtensions
    {
        public static void AddFamilyModule(this IServiceCollection services)
        {
            services.AddGrpcFamilyServiceClient();

            services.AddFileConfiguration<FamilyConfiguration>();
            services.TryAddSingleton<IFamilyManager, FamilyManager>();
            services.TryAddSingleton<IFamilyLogManager, FamilyLogManager>();
            services.TryAddSingleton<IFamilyExperienceManager, FamilyExperienceManager>();
            services.TryAddSingleton<IFamilyWarehouseManager, FamilyWarehouseManager>();

            services.AddHostedService<FamilyBuffSystem>();
            services.AddHostedService<FamilyLogSystem>();
            services.AddHostedService<FamilyExperienceSystem>();

            services.AddMessagePublisher<FamilyInviteMessage>();
            services.AddMessageSubscriber<FamilyInviteMessage, FamilyMemberInviteMessageConsumer>();
            services.AddSingleton<FamilyChatMessageConsumer>();
            services.AddMessagePublisher<FamilyChatMessage>();
            services.AddMessageSubscriber<FamilyChatMessage, FamilyChatMessageConsumer>();

            services.AddMessagePublisher<FamilyShoutMessage>();
            services.AddMessageSubscriber<FamilyShoutMessage, FamilyShoutMessageConsumer>();

            services.AddMessageSubscriber<FamilyCreateBuffMessage, FamilyCreateBuffMessageConsumer>();
            services.AddMessageSubscriber<FamilyMemberAddedMessage, FamilyMemberAddedMessageConsumer>();
            services.AddMessageSubscriber<FamilyCreatedMessage, FamilyCreatedMessageConsumer>();
            services.AddMessageSubscriber<FamilyDisbandMessage, FamilyDisbandMessageConsumer>();
            services.AddMessageSubscriber<FamilyChangeFactionMessage, FamilyChangeFactionMessageConsumer>();
            services.AddMessageSubscriber<FamilyMemberUpdateMessage, FamilyMemberUpdateMessageConsumer>();
            services.AddMessageSubscriber<FamilyMemberRemovedMessage, FamilyMemberRemovedMessageConsumer>();
            services.AddMessageSubscriber<FamilyAcknowledgeLogsMessage, FamilyAcknowledgeLogsMessageConsumer>();
            services.AddMessageSubscriber<FamilyAcknowledgeExperienceGainedMessage, FamilyAcknowledgeExperiencesMessageConsumer>();
            services.AddMessageSubscriber<FamilyUpdateMessage, FamilyUpdateMessageConsumer>();
            services.AddMessageSubscriber<FamilyCharacterJoinMessage, FamilyCharacterJoinMessageConsumer>();
            services.AddMessageSubscriber<FamilyCharacterLeaveMessage, FamilyCharacterLeaveMessageConsumer>();

            services.AddMessageSubscriber<FamilyWarehouseItemUpdateMessage, FamilyWarehouseItemUpdateMessageConsumer>();
            services.AddMessageSubscriber<FamilyWarehouseLogAddMessage, FamilyWarehouseLogAddMessageConsumer>();

            services.AddMessagePublisher<FamilyDeclareLogsMessage>();
            services.AddMessagePublisher<FamilyDeclareExperienceGainedMessage>();
            services.AddMessagePublisher<FamilyNoticeMessage>();
            services.AddMessagePublisher<FamilyMemberTodayMessage>();
            services.AddMessagePublisher<FamilyHeadSexMessage>();


            // achievements
            services.TryAddSingleton<FamilyAchievementManager>();
            services.AddSingleton<IFamilyAchievementManager, FamilyAchievementManager>(s => s.GetRequiredService<FamilyAchievementManager>());
            services.AddHostedService(s => s.GetRequiredService<FamilyAchievementManager>());
            services.AddFileConfiguration<FamilyAchievementsConfiguration>();
            services.AddMessagePublisher<FamilyAchievementIncrementMessage>();
            services.AddMessageSubscriber<FamilyAchievementUnlockedMessage, FamilyAchievementUnlockedMessageConsumer>();

            // missions
            services.AddSingleton<IFamilyMissionManager, FamilyAchievementManager>(s => s.GetRequiredService<FamilyAchievementManager>());
            services.AddFileConfiguration<FamilyMissionsConfiguration>();
            services.AddMessagePublisher<FamilyMissionIncrementMessage>();
        }
    }
}