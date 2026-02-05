using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Enrichers;
using Plugin.PlayerLogs.Enrichers.Act4;
using Plugin.PlayerLogs.Enrichers.Bazaar;
using Plugin.PlayerLogs.Enrichers.Family;
using Plugin.PlayerLogs.Enrichers.HeroicLevelUp;
using Plugin.PlayerLogs.Enrichers.Inventory;
using Plugin.PlayerLogs.Enrichers.LevelUp;
using Plugin.PlayerLogs.Enrichers.Mail;
using Plugin.PlayerLogs.Enrichers.Miniland;
using Plugin.PlayerLogs.Enrichers.Npc;
using Plugin.PlayerLogs.Enrichers.Player;
using Plugin.PlayerLogs.Enrichers.Quest;
using Plugin.PlayerLogs.Enrichers.Raid;
using Plugin.PlayerLogs.Enrichers.RainbowBattle;
using Plugin.PlayerLogs.Enrichers.Shop;
using Plugin.PlayerLogs.Enrichers.Upgrade;
using Plugin.PlayerLogs.Messages;
using Plugin.PlayerLogs.Messages.Act4;
using Plugin.PlayerLogs.Messages.Bazaar;
using Plugin.PlayerLogs.Messages.Family;
using Plugin.PlayerLogs.Messages.HeroicLevelUp;
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
using WingsAPI.Plugins;
using WingsEmu.Game;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Act7.CarvedRunes;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Logs;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Game.Warehouse.Events;

namespace Plugin.PlayerLogs
{
    public class PlayerLoggingDependencyPlugin : IGameServerPlugin
    {
        public string Name => nameof(PlayerLoggingDependencyPlugin);


        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddSingleton<IPlayerLogManager, PlayerLogManager>();
            services.AddSingleton<IHostedService>(provider => (PlayerLogManager)provider.GetService<IPlayerLogManager>());

            // Act4
            services.AddPlayerLog<Act4KillEvent, LogAct4PvpKillMessage, LogAct4PvpKillMessageEnricher>();
            services.AddPlayerLog<Act4DungeonStartedEvent, LogAct4DungeonStartedMessage, LogAct4DungeonStartedMessageEnricher>();
            services.AddPlayerLog<Act4FamilyDungeonWonEvent, LogAct4FamilyDungeonWonMessage, LogAct4FamilyDungeonWonMessageEnricher>();

            // anticheat
            services.AddPlayerLog<CharacterDisconnectedEvent, LogPlayerDisconnectedMessage, LogPlayerDisconnectedMessageEnricher>();
            services.AddPlayerLog<ExchangeCompletedEvent, LogPlayerExchangeMessage, LogPlayerExchangeMessageEnricher>();
            services.AddPlayerLog<ChatGenericEvent, LogPlayerChatMessage, LogPlayerChatMessageEnricher>();
            services.AddPlayerLog<StrangeBehaviorEvent, LogStrangeBehaviorMessage, LogStrangeBehaviorMessageEnricher>();

            // Commands
            services.AddPlayerLog<PlayerCommandEvent, LogPlayerCommandExecutedMessage, LogPlayerCommandExecutedMessageEnricher>();
            services.AddPlayerLog<GmCommandEvent, LogGmCommandExecutedMessage, LogGmCommandExecutedMessageEnricher>();

            // Level ups
            services.AddPlayerLog<LevelUpEvent, LogLevelUpCharacterMessage, LogLevelUpCharacterMessageEnricher>();
            services.AddPlayerLog<LevelUpMateEvent, LogLevelUpNosMateMessage, LogLevelUpNosMateMessageEnricher>();
            
            // Heroic level ups
            services.AddPlayerLog<HeroicLevelUpMateEvent, LogHeroicLevelUpNosMateMessage, LogHeroicLevelUpNosMateMessageEnricher>();

            // Families
            services.AddPlayerLog<FamilyCreatedEvent, LogFamilyCreatedMessage, LogFamilyCreatedMessageEnricher>();
            services.AddPlayerLog<FamilyDisbandedEvent, LogFamilyDisbandedMessage, LogFamilyDisbandedMessageEnricher>();
            services.AddPlayerLog<FamilyJoinedEvent, LogFamilyJoinedMessage, LogFamilyJoinedMessageEnricher>();
            services.AddPlayerLog<FamilyLeftEvent, LogFamilyLeftMessage, LogFamilyLeftMessageEnricher>();
            services.AddPlayerLog<FamilyMessageSentEvent, LogFamilyMessageMessage, LogFamilyMessageMessageEnricher>();
            services.AddPlayerLog<FamilyKickedMemberEvent, LogFamilyKickedMessage, LogFamilyKickedMessageEnricher>();
            services.AddPlayerLog<FamilyUpgradeBoughtEvent, LogFamilyUpgradeBoughtMessage, LogFamilyUpgradeBoughtMessageEnricher>();
            services.AddPlayerLog<FamilyWarehouseItemPlacedEvent, LogFamilyWarehouseItemPlacedMessage, LogFamilyWarehouseItemPlacedMessageEnricher>();
            services.AddPlayerLog<FamilyWarehouseItemWithdrawnEvent, LogFamilyWarehouseItemWithdrawnMessage, LogFamilyWarehouseItemWithdrawnMessageEnricher>();

            // Warehouse
            services.AddPlayerLog<WarehouseItemPlacedEvent, LogWarehouseItemPlacedMessage, LogWarehouseItemPlacedMessageEnricher>();
            services.AddPlayerLog<WarehouseItemWithdrawnEvent, LogWarehouseItemWithdrawnMessage, LogWarehouseItemWithdrawnMessageEnricher>();

            // Mini-games
            services.AddPlayerLog<MinigameRewardClaimedEvent, LogMinigameRewardClaimedMessage, LogMinigameRewardClaimedMessageEnricher>();
            services.AddPlayerLog<MinigameScoreLogEvent, LogMinigameScoreMessage, LogMinigameScoreMessageEnricher>();

            // Items
            services.AddPlayerLog<ItemGambledEvent, LogItemGambledMessage, LogItemGambledMessageEnricher>();
            services.AddPlayerLog<ItemUpgradedEvent, LogItemUpgradedMessage, LogItemUpgradedMessageEnricher>();
            services.AddPlayerLog<SpUpgradedEvent, LogSpUpgradedMessage, LogSpUpgradedMessageEnricher>();
            services.AddPlayerLog<SpPerfectedEvent, LogSpPerfectedMessage, LogSpPerfectedMessageEnricher>();
            services.AddPlayerLog<ItemSummedEvent, LogItemSummedMessage, LogItemSummedMessageEnricher>();
            services.AddPlayerLog<ShellIdentifiedEvent, LogShellIdentifiedMessage, LogShellIdentifiedMessageEnricher>();
            services.AddPlayerLog<CellonUpgradedEvent, LogCellonUpgradedMessage, LogCellonUpgradedMessageEnricher>();
            services.AddPlayerLog<BoxOpenedEvent, LogBoxOpenedMessage, LogBoxOpenedMessageEnricher>();
            services.AddPlayerLog<WeaponCarvedRuneUpgradedEvent, LogWeaponCarvedRunesUpgradeMessage, LogWeaponCarvedRunesMessageEnricher>();

            // Raid Management
            services.AddPlayerLog<RaidStartedEvent, LogRaidStartedMessage, LogRaidStartedMessageEnricher>();
            services.AddPlayerLog<RaidCreatedEvent, LogRaidCreatedMessage, LogRaidCreatedMessageEnricher>();
            services.AddPlayerLog<RaidAbandonedEvent, LogRaidAbandonedMessage, LogRaidAbandonedMessageEnricher>();
            services.AddPlayerLog<RaidLeftEvent, LogRaidLeftMessage, LogRaidLeftMessageEnricher>();
            services.AddPlayerLog<RaidJoinedEvent, LogRaidJoinedMessage, LogRaidJoinedMessageEnricher>();

            // Raid Actions
            services.AddPlayerLog<RaidSwitchButtonToggledEvent, LogRaidSwitchButtonToggledMessage, LogRaidSwitchButtonToggledEnricher>();
            services.AddPlayerLog<RaidTargetKilledEvent, LogRaidTargetKilledMessage, LogRaidTargetKilledMessageEnricher>();
            services.AddPlayerLog<RaidDiedEvent, LogRaidDiedMessage, LogRaidDiedMessageEnricher>();
            services.AddPlayerLog<RaidRewardReceivedEvent, LogRaidRewardReceivedMessage, LogRaidRewardReceivedMessageEnricher>();
            services.AddPlayerLog<RaidRevivedEvent, LogRaidRevivedMessage, LogRaidRevivedMessageEnricher>();
            services.AddPlayerLog<RaidWonEvent, LogRaidWonMessage, LogRaidWonMessageEnricher>();
            services.AddPlayerLog<RaidLostEvent, LogRaidLostMessage, LogRaidLostMessageEnricher>();

            // Quests
            //services.AddPlayerLog<QuestAddedEvent, LogQuestAddedMessage, LogQuestAddedMessageEnricher>();
            services.AddPlayerLog<QuestAbandonedEvent, LogQuestAbandonedMessage, LogQuestAbandonedMessageEnricher>();
            services.AddPlayerLog<QuestCompletedLogEvent, LogQuestCompletedMessage, LogQuestCompletedMessageEnricher>();
            //services.AddPlayerLog<QuestObjectiveUpdatedEvent, LogQuestObjectiveUpdatedMessage, LogQuestObjectiveUpdatedMessageEnricher>();

            // Shops
            services.AddPlayerLog<ShopPlayerBoughtItemEvent, LogShopPlayerBoughtItemMessage, LogShopPlayerBoughtItemMessageEnricher>();
            services.AddPlayerLog<ShopOpenedEvent, LogShopOpenedMessage, LogShopOpenedMessageEnricher>();
            services.AddPlayerLog<ShopClosedEvent, LogShopClosedMessage, LogShopClosedMessageEnricher>();
            services.AddPlayerLog<ShopNpcBoughtItemEvent, LogShopNpcBoughtItemMessage, LogShopNpcBoughtItemMessageEnricher>();
            services.AddPlayerLog<ShopNpcSoldItemEvent, LogShopNpcSoldItemMessage, LogShopNpcSoldItemMessageEnricher>();
            services.AddPlayerLog<ShopSkillBoughtEvent, LogShopSkillBoughtMessage, LogShopSkillBoughtMessageEnricher>();
            services.AddPlayerLog<ShopSkillSoldEvent, LogShopSkillSoldMessage, LogShopSkillSoldMessageEnricher>();

            // Inventory
            services.AddPlayerLog<InventoryPickedUpItemEvent, LogInventoryPickedUpItemMessage, LogInventoryPickedUpItemMessageEnricher>();
            services.AddPlayerLog<InventoryPickedUpPlayerItemEvent, LogInventoryPickedUpPlayerItemMessage, LogInventoryPickedUpPlayerItemMessageEnricher>();
            services.AddPlayerLog<InventoryItemUsedEvent, LogInventoryItemUsedMessage, LogInventoryItemUsedMessageEnricher>();
            services.AddPlayerLog<InventoryItemDeletedEvent, LogInventoryItemDeletedMessage, LogInventoryItemDeletedMessageEnricher>();

            // Invitations
            services.AddPlayerLog<TradeRequestedEvent, LogTradeRequestedMessage, LogTradeRequestedMessageEnricher>();
            services.AddPlayerLog<GroupInvitedEvent, LogGroupInvitedMessage, LogGroupInvitedMessageEnricher>();
            services.AddPlayerLog<FamilyInvitedEvent, LogFamilyInvitedMessage, LogFamilyInvitedMessageEnricher>();
            services.AddPlayerLog<RaidInvitedEvent, LogRaidInvitedMessage, LogRaidInvitedMessageEnricher>();

            // Bazaar
            services.AddPlayerLog<BazaarItemInsertedEvent, LogBazaarItemInsertedMessage, LogBazaarItemInsertedMessageEnricher>();
            services.AddPlayerLog<BazaarItemBoughtEvent, LogBazaarItemBoughtMessage, LogBazaarItemBoughtMessageEnricher>();
            services.AddPlayerLog<BazaarItemWithdrawnEvent, LogBazaarItemWithdrawnMessage, LogBazaarItemWithdrawnMessageEnricher>();
            services.AddPlayerLog<BazaarItemExpiredEvent, LogBazaarItemExpiredMessage, LogBazaarItemExpiredMessageEnricher>();

            // Mails
            services.AddPlayerLog<MailClaimedEvent, LogMailClaimedMessage, LogMailClaimedMessageEnricher>();
            services.AddPlayerLog<MailRemovedEvent, LogMailRemovedMessage, LogMailRemovedMessageEnricher>();

            // Notes
            services.AddPlayerLog<NoteSentEvent, LogNoteSentMessage, LogNoteSentMessageEnricher>();

            // Npc
            services.AddPlayerLog<ItemProducedEvent, LogItemProducedMessage, LogItemProducedMessageEnricher>();

            // Rainbow Battle
            services.AddPlayerLog<RainbowBattleWonEvent, LogRainbowBattleWonMessage, LogRainbowBattleWonMessageEnricher>();
            services.AddPlayerLog<RainbowBattleLoseEvent, LogRainbowBattleLoseMessage, LogRainbowBattleLoseMessageEnricher>();
            services.AddPlayerLog<RainbowBattleTieEvent, LogRainbowBattleTieMessage, LogRainbowBattleTieMessageEnricher>();
            services.AddPlayerLog<RainbowBattleJoinEvent, LogRainbowBattleJoinMessage, LogRainbowBattleJoinMessageEnricher>();
            services.AddPlayerLog<RainbowBattleFrozenEvent, LogRainbowBattleFrozenMessage, LogRainbowBattleFrozenMessageEnricher>();
        }
    }
}