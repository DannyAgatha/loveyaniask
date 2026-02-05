using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using WingsAPI.Communication.Compliments;
using WingsAPI.Communication.InstantBattle;
using WingsAPI.Communication.Item;
using WingsAPI.Communication.LandOfLife;
using WingsAPI.Communication.Miniland;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.Quests;
using WingsAPI.Communication.Raid;
using WingsAPI.Communication.Rewards;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.Translations;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Communication;
using WingsAPI.Plugins;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Matchmaking;
using WingsEmu.Game.GameEvent.Matchmaking.Matchmaker;
using WingsEmu.Plugins.GameEvents.Configuration;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.Consumers;
using WingsEmu.Plugins.GameEvents.Matchmaking.Matchmaker;
using WingsEmu.Plugins.GameEvents.RecurrentJob;

namespace WingsEmu.Plugins.GameEvents
{
    public class GameEventsPluginCore : IGameServerPlugin
    {
        public string Name => nameof(GameEventsPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddEventHandlersInAssembly<GameEventsPluginCore>();

            services.AddMessagePublisher<InstantBattleStartMessage>();
            services.AddMessageSubscriber<InstantBattleStartMessage, InstantBattleStartMessageConsumer>();
            services.AddMessageSubscriber<MinigameRefreshProductionPointsMessage, MinigameRefreshProductionPointsMessageConsumer>();
            services.AddMessageSubscriber<RaidRestrictionRefreshMessage, RaidRestrictionRefreshMessageConsumer>();
            services.AddMessageSubscriber<ItemRestrictionRefreshMessage, ItemRestrictionRefreshMessageConsumer>();
            services.AddMessageSubscriber<QuestDailyRefreshMessage, QuestDailyRefreshMessageConsumer>();
            services.AddMessageSubscriber<ComplimentsMonthlyRefreshMessage, ComplimentsMonthlyRefreshMessageConsumer>();
            services.AddMessageSubscriber<SpecialistPointsRefreshMessage, SpecialistPointsRefreshMessageConsumer>();
            services.AddMessageSubscriber<RankingRefreshMessage, RankingRefreshMessageConsumer>();
            services.AddMessagePublisher<TranslationsRefreshMessage>();
            services.AddMessageSubscriber<TranslationsRefreshMessage, TranslationsRefreshMessageConsumer>();
            services.AddMessageSubscriber<RewardRestrictionRefreshMessage, RewardRestrictionRefreshMessageConsumer>();
            services.AddMessagePublisher<AlzanorStartMessage>();
            services.AddMessageSubscriber<AlzanorStartMessage, AlzanorStartMessageConsumer>();
            services.AddSingleton<IGameEventRegistrationManager, GameEventRegistrationManager>();
            services.AddSingleton<IGameEventInstanceManager, GameEventInstanceManager>();

            if (gameServer.Type != GameChannelType.ACT_4)
            {
                services.AddHostedService<GameEventSystem>();
            }

            services.AddConfigurationsFromDirectory<InstantBattleConfiguration>("gameevents/instant_combats");
            services.AddSingleton<IGlobalInstantBattleConfiguration>(s => new GlobalInstantBattleConfiguration
            {
                Configurations = s.GetRequiredService<IEnumerable<InstantBattleConfiguration>>().ToList()
            });
            services.AddSingleton<IMatchmaking, Matchmaking.Matchmaking>(s => new Matchmaking.Matchmaking(new Dictionary<GameEventType, IMatchmaker>
            {
                [GameEventType.InstantBattle] = new InstantBattleMatchmaker(s.GetService<IGlobalInstantBattleConfiguration>())
            }));
        }
    }
}