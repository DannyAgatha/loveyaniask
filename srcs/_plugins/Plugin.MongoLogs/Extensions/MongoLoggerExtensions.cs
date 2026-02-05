using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.MongoLogs.Services;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs;
using Plugin.PlayerLogs.Messages.Act4;
using Plugin.PlayerLogs.Messages.Bazaar;
using Plugin.PlayerLogs.Messages.Family;
using Plugin.PlayerLogs.Messages.Inventory;
using Plugin.PlayerLogs.Messages.LevelUp;
using Plugin.PlayerLogs.Messages.Mail;
using Plugin.PlayerLogs.Messages.Miniland;
using Plugin.PlayerLogs.Messages.Npc;
using Plugin.PlayerLogs.Messages.Player;
using Plugin.PlayerLogs.Messages.Quest;
using Plugin.PlayerLogs.Messages.Raid;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using Plugin.PlayerLogs.Messages.Shop;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Extensions
{
    public static class MongoLoggerExtensions
    {
        public static void AddMongoLogs<T>(this IServiceCollection services) where T : class, IPlayerActionLogMessage
        {
            Log.Info($"Registering {typeof(T).Name} logs into Mongo storage...");
            services.AddMessageSubscriber<T, GenericLogConsumer<T>>();
        }

        public static void AddMongoLogsPlugin(this IServiceCollection services)
        {
            var pack = new ConventionPack
            {
                new TypedIgnoreDefaultPropertiesConvention<ItemInstanceDTO>()
            };
            ConventionRegistry.Register("Ignore Default Pack", pack, t => true);


            services.AddMongoLogs<LogPlayerExchangeMessage>();
            services.AddMongoLogs<LogPlayerChatMessage>();

            // Connection
            services.AddMongoLogs<LogPlayerDisconnectedMessage>();

            // Act4
            services.AddMongoLogs<LogAct4PvpKillMessage>();
            services.AddMongoLogs<LogAct4DungeonStartedMessage>();
            services.AddMongoLogs<LogAct4FamilyDungeonWonMessage>();

            // Level ups
            services.AddMongoLogs<LogLevelUpCharacterMessage>();
            services.AddMongoLogs<LogLevelUpNosMateMessage>();

            // Family
            services.AddMongoLogs<LogFamilyCreatedMessage>();
            services.AddMongoLogs<LogFamilyDisbandedMessage>();
            services.AddMongoLogs<LogFamilyJoinedMessage>();
            services.AddMongoLogs<LogFamilyLeftMessage>();
            services.AddMongoLogs<LogFamilyMessageMessage>();
            services.AddMongoLogs<LogFamilyKickedMessage>();
            services.AddMongoLogs<LogFamilyUpgradeBoughtMessage>();
            services.AddMongoLogs<LogFamilyWarehouseItemPlacedMessage>();
            services.AddMongoLogs<LogFamilyWarehouseItemWithdrawnMessage>();

            // Raids
            services.AddMongoLogs<LogRaidStartedMessage>();
            services.AddMongoLogs<LogRaidCreatedMessage>();
            services.AddMongoLogs<LogRaidAbandonedMessage>();
            services.AddMongoLogs<LogRaidLeftMessage>();
            services.AddMongoLogs<LogRaidJoinedMessage>();
            services.AddMongoLogs<LogRaidSwitchButtonToggledMessage>();
            services.AddMongoLogs<LogRaidTargetKilledMessage>();
            services.AddMongoLogs<LogRaidDiedMessage>();
            services.AddMongoLogs<LogRaidRewardReceivedMessage>();
            services.AddMongoLogs<LogRaidRevivedMessage>();
            services.AddMongoLogs<LogRaidWonMessage>();
            services.AddMongoLogs<LogRaidLostMessage>();

            // Rainbow Battle
            services.AddMongoLogs<LogRainbowBattleWonMessage>();
            services.AddMongoLogs<LogRainbowBattleLoseMessage>();
            services.AddMongoLogs<LogRainbowBattleTieMessage>();
            services.AddMongoLogs<LogRainbowBattleJoinMessage>();
            services.AddMongoLogs<LogRainbowBattleFrozenMessage>();
            
            // Mini-games
            services.AddMongoLogs<LogMinigameRewardClaimedMessage>();
            services.AddMongoLogs<LogMinigameScoreMessage>();

            // Items
            services.AddMongoLogs<LogItemGambledMessage>();
            services.AddMongoLogs<LogItemUpgradedMessage>();
            services.AddMongoLogs<LogSpUpgradedMessage>();
            services.AddMongoLogs<LogItemSummedMessage>();
            services.AddMongoLogs<LogShellIdentifiedMessage>();
            services.AddMongoLogs<LogCellonUpgradedMessage>();
            services.AddMongoLogs<LogSpPerfectedMessage>();
            services.AddMongoLogs<LogBoxOpenedMessage>();

            // Quests
            //services.AddMongoLogs<LogQuestAddedMessage>();
            services.AddMongoLogs<LogQuestAbandonedMessage>();
            services.AddMongoLogs<LogQuestCompletedMessage>();
            //services.AddMongoLogs<LogQuestObjectiveUpdatedMessage>();

            // Shops
            services.AddMongoLogs<LogShopPlayerBoughtItemMessage>();
            services.AddMongoLogs<LogShopOpenedMessage>();
            services.AddMongoLogs<LogShopClosedMessage>();
            services.AddMongoLogs<LogShopNpcBoughtItemMessage>();
            services.AddMongoLogs<LogShopNpcSoldItemMessage>();
            services.AddMongoLogs<LogShopSkillBoughtMessage>();
            services.AddMongoLogs<LogShopSkillSoldMessage>();

            // Inventory
            services.AddMongoLogs<LogInventoryPickedUpItemMessage>();
            services.AddMongoLogs<LogInventoryPickedUpPlayerItemMessage>();
            services.AddMongoLogs<LogInventoryItemUsedMessage>();
            services.AddMongoLogs<LogInventoryItemDeletedMessage>();

            // Invitations
            services.AddMongoLogs<LogTradeRequestedMessage>();
            services.AddMongoLogs<LogGroupInvitedMessage>();
            services.AddMongoLogs<LogFamilyInvitedMessage>();
            services.AddMongoLogs<LogRaidInvitedMessage>();

            // Bazaar
            services.AddMongoLogs<LogBazaarItemInsertedMessage>();
            services.AddMongoLogs<LogBazaarItemBoughtMessage>();
            services.AddMongoLogs<LogBazaarItemExpiredMessage>();
            services.AddMongoLogs<LogBazaarItemWithdrawnMessage>();

            // Warehouse
            services.AddMongoLogs<LogWarehouseItemWithdrawnMessage>();
            services.AddMongoLogs<LogWarehouseItemPlacedMessage>();

            // Mails
            services.AddMongoLogs<LogMailClaimedMessage>();
            services.AddMongoLogs<LogMailRemovedMessage>();
            services.AddMongoLogs<LogMailSentMessage>();

            // Notes
            services.AddMongoLogs<LogNoteSentMessage>();

            // Npc
            services.AddMongoLogs<LogItemProducedMessage>();

            services.AddSingleton(new MongoLogsBackgroundService(MongoLogsConfiguration.FromEnv()));
            services.AddSingleton<IHostedService>(provider => provider.GetService<MongoLogsBackgroundService>());
            
            var objectSerializer = new ObjectSerializer(type => type.FullName != null && (ObjectSerializer.DefaultAllowedTypes(type) || type.FullName.StartsWith("Plugin.MongoLogs.Entities")));
            BsonSerializer.RegisterSerializer(objectSerializer);
        }
    }
}