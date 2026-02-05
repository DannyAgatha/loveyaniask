// NosEmu
// 


using System;
using Microsoft.Extensions.DependencyInjection;
using Plugin.ResourceLoader.Loaders;
using WingsAPI.Data.ActDesc;
using WingsAPI.Data.Fish;
using WingsAPI.Data.GameData;
using WingsAPI.Plugins;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;

namespace Plugin.ResourceLoader
{
    public class FileResourceLoaderPlugin : IDependencyInjectorPlugin
    {
        public string Name => nameof(FileResourceLoaderPlugin);

        public void AddDependencies(IServiceCollection services)
        {
            services.AddSingleton(s => new ResourceLoadingConfiguration(Environment.GetEnvironmentVariable("WINGSEMU_RESOURCE_PATH") ?? "resources"));
            services.AddSingleton<IResourceLoader<ItemDTO>, ItemResourceFileLoader>();
            services.AddSingleton<IResourceLoader<SkillDTO>, SkillResourceFileLoader>();
            services.AddSingleton<IResourceLoader<NpcMonsterDto>, NpcMonsterFileLoader>();
            services.AddSingleton<IResourceLoader<CardDTO>, CardResourceFileLoader>();
            services.AddSingleton<IResourceLoader<MapDataDTO>, MapResourceFileLoader>();
            services.AddSingleton<IResourceLoader<QuestDto>, QuestResourceFileLoader>();
            services.AddSingleton<IResourceLoader<TutorialDto>, TutorialResourceFileLoader>();
            services.AddSingleton<IResourceLoader<QuestNpcDto>, NpcQuestResourceFileLoader>();
            services.AddSingleton<IResourceLoader<ActDescDTO>, ActDescResourceFileLoader>();
            services.AddSingleton<IResourceLoader<FishingSpotDto>, FishFileLoader>();
            services.AddSingleton<IResourceLoader<GameDataTranslationDto>, GameDataLanguageFileLoader>();

            services.AddSingleton<IGameDataLanguageService, InMemoryGameDataLanguageService>();

            services.AddSingleton<IBattleEntityAlgorithmService, BattleEntityAlgorithmService>();
        }
    }

    public class GameResourceLoaderPlugin : IGameServerPlugin
    {
        public string Name => nameof(FileResourceLoaderPlugin);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddSingleton<IResourceLoader<GenericTranslationDto>, GenericTranslationGrpcLoader>();
            services.AddSingleton<IGameLanguageService, InMemoryMultilanguageService>();
        }
    }
}