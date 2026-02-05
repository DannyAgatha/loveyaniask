using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using Plugin.QuestImpl.Managers;
using WingsAPI.Plugins;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Configurations;

namespace Plugin.QuestImpl
{
    public class QuestPluginCore : IGameServerPlugin
    {
        public string Name => nameof(QuestPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddEventHandlersInAssembly<QuestPluginCore>();
            services.AddRunScriptHandlers();

            services.TryAddSingleton<IQuestManager, QuestManager>();
            services.TryAddSingleton<IQuestFactory, QuestFactory>();

            services.TryAddSingleton<INpcRunTypeQuestsConfiguration, NpcRunTypeQuestsConfiguration>();
            services.TryAddSingleton<IPeriodicQuestsConfiguration>(s => s.GetRequiredService<PeriodicQuestsConfiguration>());

            services.AddFileConfiguration<QuestsRatesConfiguration>("quests_rates_configuration");
            services.AddFileConfiguration<GeneralQuestsConfiguration>("general_quests_configuration");
            services.AddFileConfiguration<SoundFlowerConfiguration>("sound_flower_configuration");

            services.AddMultipleConfigurationOneFile<NpcRunTypeQuestsInfo>("npc_run_type_quests_configuration");

            services.AddFileConfiguration<QuestTeleportDialogConfiguration>("quest_teleport_dialog_configuration");
            services.AddFileConfiguration(new PeriodicQuestsConfiguration
            {
                DailyQuests = new HashSet<PeriodicQuestSet>
                {
                    new()
                }
            });
        }
    }
}