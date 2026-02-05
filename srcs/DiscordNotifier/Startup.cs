using System;
using DiscordNotifier.Consumers.Chat;
using DiscordNotifier.Consumers.Family;
using DiscordNotifier.Consumers.GameEvents;
using DiscordNotifier.Consumers.Item;
using DiscordNotifier.Consumers.Maintenance;
using DiscordNotifier.Consumers.Minigame;
using DiscordNotifier.Consumers.Player;
using DiscordNotifier.Discord;
using DiscordNotifier.Formatting;
using DiscordNotifier.Managers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using Plugin.PlayerLogs.Messages;
using Plugin.PlayerLogs.Messages.Family;
using Plugin.PlayerLogs.Messages.LevelUp;
using Plugin.PlayerLogs.Messages.Miniland;
using Plugin.PlayerLogs.Messages.Player;
using Plugin.PlayerLogs.Messages.Upgrade;
using Plugin.ResourceLoader;
using WingsAPI.Communication.InstantBattle;
using WingsAPI.Communication.Services.Messages;
using WingsAPI.Communication.WorldBoss;
using WingsEmu.Health.Extensions;

namespace DiscordNotifier
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMqttConfigurationFromEnv();
            services.AddEventPipeline();
            services.AddEventHandlersInAssembly<Startup>();
            services.AddMaintenanceMode();
            services.AddPhoenixLogging();
            services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));

            new FileResourceLoaderPlugin().AddDependencies(services);

            // discord
            services.AddYamlConfigurationHelper();
            services.AddSingleton(new DiscordWebhookConfiguration
            {
                { LogType.PLAYERS_EVENTS_CHANNEL, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_CREATE_URL") ?? "https://discord.com/api/webhooks/1404764738608562237/P_Hrut1KCF4XqZnXVtPXeovwkWxE8ZMky34WvoyS9Hr1fXqknNpqOcRUQ0bx_485Un5y" },
                { LogType.CHAT_FAMILIES, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_CHAT_FAMILIES") ?? "https://discord.com/api/webhooks/1404763321713954846/25zp17M-8F0dviwyxgUqeHB8qjksU22cB3zE1UGx7JmgpOO8EXtdRquAj967kuRj9qFH"},
                { LogType.CHAT_FRIENDS, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_CHAT_FRIENDS") ?? "https://discord.com/api/webhooks/1404763321713954846/25zp17M-8F0dviwyxgUqeHB8qjksU22cB3zE1UGx7JmgpOO8EXtdRquAj967kuRj9qFH"},
                { LogType.CHAT_GENERAL, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_CHAT_GENERAL") ?? "https://discord.com/api/webhooks/1404763321713954846/25zp17M-8F0dviwyxgUqeHB8qjksU22cB3zE1UGx7JmgpOO8EXtdRquAj967kuRj9qFH"},
                { LogType.CHAT_GROUPS, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_CHAT_GROUPS") ?? "https://discord.com/api/webhooks/1404763321713954846/25zp17M-8F0dviwyxgUqeHB8qjksU22cB3zE1UGx7JmgpOO8EXtdRquAj967kuRj9qFH"},
                { LogType.CHAT_SPEAKERS, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_CHAT_SPEAKERS") ?? "https://discord.com/api/webhooks/1404763321713954846/25zp17M-8F0dviwyxgUqeHB8qjksU22cB3zE1UGx7JmgpOO8EXtdRquAj967kuRj9qFH"},
                { LogType.CHAT_WHISPERS, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_CHAT_WHISPERS") ?? "https://discord.com/api/webhooks/1404763321713954846/25zp17M-8F0dviwyxgUqeHB8qjksU22cB3zE1UGx7JmgpOO8EXtdRquAj967kuRj9qFH"},
                { LogType.FARMING_LEVEL_UP, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FARMING_LEVEL_UP") ?? "https://discord.com/api/webhooks/1404764978724212816/IvRGrJvlHdG3Zm0S2-BBEf-kxLf_P32eEQm3haFQSF_mnimaU1URVh5ma9oMakuakh5X"},
                { LogType.COMMANDS_PLAYER_COMMAND_EXECUTED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_PLAYER_COMMAND_EXECUTED") ?? "https://discord.com/api/webhooks/1404763554329923617/KM3qJ-ZjJebusZAkz5rrOwLaBHnDbpuj_ZwfbZ8cj2gjhxuf2GVmRz9wDNMbEGZSu6F-"},
                { LogType.COMMANDS_GM_COMMAND_EXECUTED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_GM_COMMAND_EXECUTED") ?? "https://discord.com/api/webhooks/1404763554329923617/KM3qJ-ZjJebusZAkz5rrOwLaBHnDbpuj_ZwfbZ8cj2gjhxuf2GVmRz9wDNMbEGZSu6F-"},
                { LogType.FAMILY_CREATED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_CREATED") ?? "https://discord.com/api/webhooks/1404763706558124113/K5WNe6A_QvEgihDGMFJa4jQIcQmjjqpRKEHgmGraf5-P5skmiShe5eRryXyUT4CdiOiw"},
                { LogType.FAMILY_DISBANDED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_DISBANDED") ?? "https://discord.com/api/webhooks/1404763706558124113/K5WNe6A_QvEgihDGMFJa4jQIcQmjjqpRKEHgmGraf5-P5skmiShe5eRryXyUT4CdiOiw"},
                { LogType.FAMILY_JOINED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_JOINED") ?? "https://discord.com/api/webhooks/1404763706558124113/K5WNe6A_QvEgihDGMFJa4jQIcQmjjqpRKEHgmGraf5-P5skmiShe5eRryXyUT4CdiOiw"},
                { LogType.FAMILY_LEFT, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_LEFT") ?? "https://discord.com/api/webhooks/1404763706558124113/K5WNe6A_QvEgihDGMFJa4jQIcQmjjqpRKEHgmGraf5-P5skmiShe5eRryXyUT4CdiOiw"},
                { LogType.FAMILY_KICK, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_KICK") ?? "https://discord.com/api/webhooks/1404763706558124113/K5WNe6A_QvEgihDGMFJa4jQIcQmjjqpRKEHgmGraf5-P5skmiShe5eRryXyUT4CdiOiw"},
                { LogType.FAMILY_MESSAGES, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_FAMILY_MESSAGES") ?? "https://discord.com/api/webhooks/1404763706558124113/K5WNe6A_QvEgihDGMFJa4jQIcQmjjqpRKEHgmGraf5-P5skmiShe5eRryXyUT4CdiOiw"},
                { LogType.MINIGAME_REWARD_CLAIMED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_MINIGAME_REWARDS_CLAIMED") ?? "https://discord.com/api/webhooks/1404764155210104872/jPjC6OKTAo58mUWglKBJm9FUH86ttDvrUJZVjsb1wwfcdG07GRBnEh9CeLBaDXv7AvXa"},
                { LogType.MINIGAME_SCORE, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_MINIGAME_SCORE") ?? "https://discord.com/api/webhooks/1404764155210104872/jPjC6OKTAo58mUWglKBJm9FUH86ttDvrUJZVjsb1wwfcdG07GRBnEh9CeLBaDXv7AvXa"},
                { LogType.ITEM_GAMBLED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_ITEM_GAMBLED") ?? "https://discord.com/api/webhooks/1404764462606585988/eFhXuNeXaZ4ElvuZ7iW1YjdFZmeVXVoIfmSfRxCqZxKf5MfLGyjN4YmXJZ6MEQYrRL8_"},
                { LogType.ITEM_UPGRADED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_ITEM_UPGRADED") ?? "https://discord.com/api/webhooks/1404764462606585988/eFhXuNeXaZ4ElvuZ7iW1YjdFZmeVXVoIfmSfRxCqZxKf5MfLGyjN4YmXJZ6MEQYrRL8_"},
                { LogType.STRANGE_BEHAVIORS, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_STRANGE_BEHAVIORS") ?? "https://discord.com/api/webhooks/1404763200632651916/1i27-gUu9c9LtUe_tN_O39U_ROldQNfV_5UmmrJMgayTizBC27v7NSzke7215bqXxfFd"},
                { LogType.WORLD_BOSS_STARTED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_WORLD_BOSS_STARTED")},
                { LogType.PRESTIGE_UNLOCKED, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_PRESTIGE_UNLOCKED")}
            });
            services.AddSingleton<IDiscordWebhookLogsService, DiscordWebhookLogsService>();
            services.AddSingleton<ItemManager>();

            new DatabasePlugin().AddDependencies(services);

            services.AddDiscordFormattedLog<LogPlayerChatMessage, LogChatMessageMessageFormatter>();
            services.AddDiscordFormattedLog<LogLevelUpCharacterMessage, LogPlayerLevelUpMessageFormatter>();
            services.AddDiscordFormattedLog<LogPlayerCommandExecutedMessage, LogPlayerCommandExecutedMessageFormatter>();
            services.AddDiscordFormattedLog<LogGmCommandExecutedMessage, LogGmCommandExecutedMessageFormatter>();

            // Family discord
            services.AddDiscordFormattedLog<LogFamilyCreatedMessage, LogFamilyCreatedMessageFormatter>();
            services.AddDiscordFormattedLog<LogFamilyDisbandedMessage, LogFamilyDisbandedMessageFormatter>();
            services.AddDiscordFormattedLog<LogFamilyJoinedMessage, LogFamilyJoinedMessageFormatter>();
            services.AddDiscordFormattedLog<LogFamilyKickedMessage, LogFamilyKickedMessageFormatter>();
            services.AddDiscordFormattedLog<LogFamilyLeftMessage, LogFamilyLeftMessageFormatter>();
            services.AddDiscordFormattedLog<LogFamilyMessageMessage, LogFamilyMessageMessageFormatter>();
            services.AddDiscordEmbedFormattedLog<LogFamilyCreatedMessage, LogFamilyCreatedEmbedMessageFormatter>();

            // Minigames discord
            services.AddDiscordFormattedLog<LogMinigameRewardClaimedMessage, LogMinigameRewardClaimedMessageFormatter>();
            services.AddDiscordFormattedLog<LogMinigameScoreMessage, LogMinigameScoreMessageFormatter>();

            // Items discord
            services.AddDiscordFormattedLog<LogItemGambledMessage, LogItemGambledMessageFormatter>();
            services.AddDiscordFormattedLog<LogItemUpgradedMessage, LogItemUpgradedMessageFormatter>();

            services.AddDiscordEmbedFormattedLog<LogStrangeBehaviorMessage, LogStrangeBehaviorEmbedFormatter>();

            services.AddMessageSubscriber<InstantBattleStartMessage, LogInstantBattleStartDiscordConsumer>();

            // healthcheck
            services.AddMessageSubscriber<ServiceDownMessage, ServiceDownMessageConsumer>();

            // maintenance
            services.AddMessageSubscriber<ServiceMaintenanceNotificationMessage, ServiceMaintenanceNotificationMessageConsumer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
        }
    }
}