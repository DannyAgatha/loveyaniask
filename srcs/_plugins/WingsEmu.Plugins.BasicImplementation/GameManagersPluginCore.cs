using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NosEmu.Plugins.BasicImplementations.Algorithms;
using NosEmu.Plugins.BasicImplementations.Arena;
using NosEmu.Plugins.BasicImplementations.Compliments;
using NosEmu.Plugins.BasicImplementations.Entities;
using NosEmu.Plugins.BasicImplementations.Event.Items;
using NosEmu.Plugins.BasicImplementations.Evtb.RecurrentJob;
using NosEmu.Plugins.BasicImplementations.Factories;
using NosEmu.Plugins.BasicImplementations.ForbiddenNames;
using NosEmu.Plugins.BasicImplementations.Inventory;
using NosEmu.Plugins.BasicImplementations.ItemUsage;
using NosEmu.Plugins.BasicImplementations.Mail;
using NosEmu.Plugins.BasicImplementations.Managers;
using NosEmu.Plugins.BasicImplementations.Managers.StaticData;
using NosEmu.Plugins.BasicImplementations.Miniland;
using NosEmu.Plugins.BasicImplementations.ServerConfigs;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Drops;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Maps;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Monsters;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Portals;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Rewards;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Teleporters;
using PhoenixLib.Caching;
using PhoenixLib.Configuration;
using PhoenixLib.DAL.Redis.Locks;
using WingsAPI.Plugins;
using WingsEmu.Game;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Arena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Act7.CarvedRunes;
using WingsEmu.Game.Configurations.Act7.Tattoos;
using WingsEmu.Game.Configurations.SetEffect;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Features;
using WingsEmu.Game.Fish;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.PartnerFusion;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Pity;
using WingsEmu.Game.Relations;
using WingsEmu.Game.Skills;
using WingsEmu.Game.SnackFood;
using NosEmu.Plugins.BasicImplementations.Bazaar;
using NosEmu.Plugins.BasicImplementations.DbServer;
using NosEmu.Plugins.BasicImplementations.InterChannel;
using NosEmu.Plugins.BasicImplementations.Ship;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Configurations.Gold;
using WingsEmu.Game.Configurations.InitialConfiguration;
using WingsEmu.Game.Configurations.LandOfLife;
using WingsEmu.Game.Configurations.LegendaryDrop;
using WingsEmu.Game.Configurations.MapTokenPoints;
using WingsEmu.Game.Configurations.MysteryBox;
using WingsEmu.Game.Configurations.PetEvolution;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Configurations.RaidExtraRewards;
using WingsEmu.Game.Configurations.Skin;
using WingsEmu.Game.Configurations.SpecialistShardExchange;
using WingsEmu.Game.Configurations.UnderWaterShowdown;
using WingsEmu.Game.Configurations.UpgradeCostume;
using WingsEmu.Game.Configurations.WorldBoss;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Plugins.GameEvents;

namespace NosEmu.Plugins.BasicImplementations;

public class GameManagersPluginCore : IGameServerPlugin
{
    public string Name => nameof(GameManagersPluginCore);


    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        // server configs
        services.AddConfigurationsFromDirectory<TeleporterImportFile>("map_teleporters");
        services.TryAddSingleton<ITeleporterManager, TeleporterManager>();

        services.AddConfigurationsFromDirectory<LevelRewardsImportFile>("level_rewards");
        services.AddConfigurationsFromDirectory<RandomBoxImportFile>("random_boxes");
        services.AddConfigurationsFromDirectory<ItemBoxImportFile>("item_boxes");
        services.TryAddSingleton<IItemBoxManager, ItemBoxManager>();

        services.AddConfigurationsFromDirectory<RecipeImportFile>("recipes");
        services.TryAddSingleton<IRecipeManager, RecipeManager>();
        services.TryAddSingleton<IRecipeFactory, RecipeFactory>();

        services.AddConfigurationsFromDirectory<DropImportFile>("global_drops");
        services.TryAddSingleton<IDropManager, DropManager>();

        services.AddConfigurationsFromDirectory<MapNpcImportFile>("map_npc_placement");
        services.TryAddSingleton<IMapNpcManager, MapNpcManager>();
        services.TryAddSingleton<IShopManager, ShopManager>();

        services.AddConfigurationsFromDirectory<MapMonsterImportFile>("map_monster_placement");
        services.TryAddSingleton<IMapMonsterManager, MapMonsterManager>();

        services.AddConfigurationsFromDirectory<PortalImportFile>("map_portals");
        services.AddConfigurationsFromDirectory<ConfiguredMapImportFile>("maps");
        services.TryAddSingleton<IMapManager, MapManager>();
        
        services.TryAddSingleton<IFishManager, FishingManager>();

        // core client data
        services.TryAddSingleton<ICardsManager, CardsManager>();
        services.TryAddSingleton<INpcMonsterManager, NpcMonsterManager>();
        services.TryAddSingleton<ISkillsManager, SkillsManager>();
        services.TryAddSingleton<IItemsManager, ItemsManager>();

        // mails 
        services.TryAddSingleton<MailCreationManager>();
        services.AddHostedService(s => s.GetRequiredService<MailCreationManager>());

        // other managers
        services.TryAddSingleton<IArenaManager, ArenaManager>();
        services.TryAddSingleton<ITeleportManager, TeleportManager>();
        services.AddBazaarModule();
        services.AddInterChannelModule();
        services.AddShipModule(gameServer);
        services.AddHostedService<EvtbSystem>();
        services.AddDbServerModule();
        services.TryAddSingleton<IRankingManager, RankingManager>();
        services.TryAddSingleton<IExpirableLockService, RedisCheckableLock>();
        services.TryAddSingleton<IServerManager, ServerManager>();
        services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));
        services.TryAddSingleton(typeof(IUuidKeyCachedRepository<>), typeof(InMemoryUuidCacheRepository<>));
        services.TryAddSingleton(typeof(IKeyValueCache<>), typeof(InMemoryKeyValueCache<>));
        services.TryAddSingleton<IRandomGenerator, RandomGenerator>();
        services.TryAddSingleton<ISessionManager, SessionManager>();
        services.TryAddSingleton<IShopFactory, ShopFactory>();
        services.TryAddSingleton<IGroupManager, GroupManager>();
        services.TryAddSingleton<IScriptedInstanceManager, ScriptedInstanceManager>();
        services.TryAddSingleton<IMinilandManager, MinilandManager>();
        services.TryAddSingleton<IMinigameManager, MinigameManager>();
        services.TryAddSingleton<IDelayConfiguration, DelayConfiguration>();
        services.TryAddSingleton<IDelayManager, DelayManager>();
        services.TryAddSingleton<IMateTransportFactory, MateTransportFactory>();
        services.TryAddSingleton<IGameEventRegistrationManager, GameEventRegistrationManager>();
        services.TryAddSingleton<IRevivalManager, RevivalManager>();
        services.TryAddSingleton<ISacrificeManager, SacrificeManager>();
        services.TryAddSingleton<IMeditationManager, MeditationManager>();
        services.TryAddSingleton<IPhantomPositionManager, PhantomPositionManager>();
        services.TryAddSingleton<IInvitationManager, InvitationManager>();
        services.TryAddSingleton<IGroupFactory, GroupFactory>();
        services.TryAddSingleton<IGameItemInstanceFactory, GameItemInstanceFactory>();
        services.TryAddSingleton<ICellonGenerationAlgorithm, CellonGenerationAlgorithm>();
        services.TryAddSingleton<IShellGenerationAlgorithm, ShellGenerationAlgorithm>();
        services.TryAddSingleton<IDamageAlgorithm, DamageAlgorithm>();
        services.TryAddSingleton<ISpyOutManager, SpyOutManager>();
        services.TryAddSingleton<IComplimentsManager, ComplimentsManager>();
        services.AddTransient<IBuffFactory, BuffFactory>();
        services.AddTransient<IFoodSnackComponentFactory, FoodSnackComponentFactory>();
        services.AddTransient<IMapDesignObjectFactory, MapDesignObjectFactory>();
        services.AddTransient<IBattleEntityDumpFactory, BattleEntityDumpFactory>();
        services.TryAddSingleton<IPlayerEntityFactory, PlayerEntityFactory>();
        services.TryAddSingleton<IMateEntityFactory, MateEntityFactory>();
        services.TryAddSingleton<IItemUsageToggleManager, RedisItemUsageToggleManager>();
        services.TryAddSingleton<IGameFeatureToggleManager, RedisGameFeatureToggleManager>();

        services.TryAddSingleton<IForbiddenNamesManager, ReloadableForbiddenNamesManager>();

        services.AddFileConfiguration<Act4Configuration>();
        services.AddFileConfiguration<SnackFoodConfiguration>("snack_food_configuration");

        services.AddFileConfiguration<RelictConfiguration>("relict_configuration");
        services.AddFileConfiguration<ItemSumConfiguration>("item_sum_configuration");
        services.AddFileConfiguration<UpgradeNormalItemConfiguration>("upgrade_normal_item_configuration");
        services.AddFileConfiguration<UpgradePhenomenalItemConfiguration>("upgrade_phenomenal_item_configuration");

        services.AddFileConfiguration<GamblingRarityInfo>("gambling_configuration");
        services.TryAddSingleton<IGamblingRarityConfiguration, GamblingRarityConfiguration>();

        services.AddFileConfiguration<DropRarityConfiguration>("drop_rarity_configuration");
        services.TryAddSingleton<IDropRarityConfigurationProvider, DropRarityConfigurationProvider>();

        services.AddFileConfiguration<PartnerSpecialistSkillRollConfiguration>();
        services.TryAddSingleton<IPartnerSpecialistSkillRoll, PartnerSpecialistSkillRoll>();

        services.AddMultipleConfigurationOneFile<TimeSpaceFileConfiguration>("time_space_configuration");
        services.TryAddSingleton<ITimeSpaceConfiguration, TimeSpaceConfiguration>();

        services.AddMultipleConfigurationOneFile<TimeSpaceNpcRunConfiguration>("time_space_npc_run_configuration");
        services.TryAddSingleton<ITimeSpaceNpcRunConfig, TimeSpaceNpcRunConfig>();

        services.AddMultipleConfigurationOneFile<ChestDropItemConfiguration>("chest_drop_item_configuration");
        services.TryAddSingleton<IChestDropItemConfig, ChestDropItemConfig>();

        services.AddMultipleConfigurationOneFile<SubActsConfiguration>("subacts_configuration");
        services.TryAddSingleton<ISubActConfiguration, SubActConfiguration>();

        services.AddFileConfiguration<BuffsToRemoveConfiguration>("buffs_to_remove_configuration");
        services.TryAddSingleton<IBuffsToRemoveConfig, BuffsToRemoveConfig>();

        services.AddMultipleConfigurationOneFile<GibberishConfiguration>("gibberish_configuration");
        services.TryAddSingleton<IGibberishConfig, GibberishConfig>();

        services.AddMultipleConfigurationOneFile<Act5NpcRunCraftItemConfig>("act5_npc_run_item_configuration");
        services.TryAddSingleton<IAct5NpcRunCraftItemConfiguration, Act5NpcRunCraftItemConfiguration>();

        services.AddMultipleConfigurationOneFile<MajorTrophyNpcRunCraftItemConfig>("major_trophy_npc_run_item_configuration");
        services.TryAddSingleton<IMajorTrophyNpcRunCraftItemConfiguration, MajorTrophyNpcRunCraftItemConfiguration>();

        services.AddMultipleConfigurationOneFile<PartnerSpecialistBasicConfiguration>("partner_specialist_basic_configuration");
        services.TryAddSingleton<IPartnerSpecialistBasicConfig, PartnerSpecialistBasicConfig>();

        services.AddMultipleConfigurationOneFile<MonsterTalkingConfiguration>("monster_talking_configuration");
        services.TryAddSingleton<IMonsterTalkingConfig, MonsterTalkingConfig>();
        
        services.AddFileConfiguration<RainbowBattleConfiguration>("gameevents/rainbow_battle/rainbow_configuration");
        services.AddFileConfiguration<RainbowBattleRewardsConfiguration>("gameevents/rainbow_battle/rainbow_battle_rewards");
        
        services.AddFileConfiguration<BattlePassConfiguration>("battlepass/battlepass_configuration");
        services.AddFileConfiguration<BattlePassQuestConfiguration>("battlepass/battlepass_quest");
        services.AddFileConfiguration<BattlePassBearingConfiguration>("battlepass/battlepass_bearing");
        services.AddFileConfiguration<BattlePassItemConfiguration>("battlepass/battlepass_item");

        services.AddFileConfiguration<TrainerConfig>("trainer_config");
        services.TryAddSingleton<ITrainerConfiguration, TrainerConfiguration>();

        services.AddFileConfiguration<LandOfDeathConfiguration>("land_of_death/land_of_death_configuration");
        services.TryAddSingleton<ILandOfDeathConfig, LandOfDeathConfig>();

        services.AddFileConfiguration<CreateFairyAct6Configuration>("act6_crafting_fairy_configuration");
        
        services.AddMultipleConfigurationOneFile<ChangeClassByTypeConfig>("change_class_configuration");
        services.TryAddSingleton<ChangeClassConfiguration>();
        services.AddFileConfiguration<PartnerFusionExperienceConfiguration>();
        services.AddFileConfiguration<PartnerFusionDataConfiguration>();
        
        services.AddFileConfiguration<PityConfiguration>("pity_system/pity_configuration");
        services.TryAddSingleton<PityConfiguration>();
        
        services.AddFileConfiguration<PrestigeConfiguration>("prestige_system/prestige_configuration");
        services.TryAddSingleton<PrestigeConfiguration>();
        
        services.AddFileConfiguration<FishConfiguration>("fishing_configuration");
        
        services.AddFileConfiguration<PetMaxLevelConfiguration>("trainer_specialist/monster_max_level");

        services.AddFileConfiguration<TrainerSpecialistFileConfiguration>("trainer_specialist/monster_spawn");
        services.TryAddSingleton<ITrainerSpecialistConfiguration, TrainerSpecialistConfiguration>();

        services.AddFileConfiguration<TrainerQuestConfiguration>("trainer_specialist/trainer_quest");
        services.AddFileConfiguration<TrainerSpecialistPetBookConfiguration>("trainer_specialist/pet_book");
        services.AddFileConfiguration<TrainerSpecialistPetSkillsLearningConfiguration>("trainer_specialist/pet_skills_learning");
        services.AddFileConfiguration<TrainerRatesConfiguration>("trainer_specialist/trainer_rates");
        services.AddFileConfiguration<ItemBuyDailyLimitConfiguration>("item_buy_daily_limit");
        
        services.AddFileConfiguration<BossScalingFileConfiguration>("boss_health_scaling");
        services.TryAddSingleton<IBossScalingConfiguration, BossScalingConfiguration>();

        services.AddFileConfiguration<RaidModeFileConfiguration>("raid_mode_type");
        services.TryAddSingleton<IRaidModeConfiguration, RaidModeConfiguration>();
        
        services.AddFileConfiguration<DailyRewardsConfiguration>("daily_rewards/daily_play_time_rewards");
        services.TryAddSingleton<DailyRewardsConfiguration>();
        
        services.AddFileConfiguration<InitialClassConfiguration>("initial_configuration/initial_class_equipment");
        services.TryAddSingleton<InitialClassConfiguration>();
        
        services.AddFileConfiguration<InitialMateConfiguration>("initial_configuration/mate_initial_configuration");
        services.TryAddSingleton<InitialMateConfiguration>();
        
        services.AddFileConfiguration<InitialCharacterJobConfiguration>("initial_configuration/initial_character_new_job_configuration");
        services.TryAddSingleton<InitialCharacterJobConfiguration>();
        
        services.AddFileConfiguration<GeneralMapGoldConfiguration>("game_configuration/map_gold_configuration");
        services.TryAddSingleton<GeneralMapGoldConfiguration>();
        
        services.AddFileConfiguration<RaidExtraRewardsConfiguration>("raid_special_configuration/raid_extra_rewards");
        services.TryAddSingleton<RaidExtraRewardsConfiguration>();
        
        services.AddFileConfiguration<FashionUpgradeConfiguration>("fashion_upgrade/fashion_upgrade_configuration");
        services.TryAddSingleton<FashionUpgradeConfiguration>();
        
        services.AddFileConfiguration<SkinRevealConfiguration>("skin_boxes/skins_class_configuration");
        services.TryAddSingleton<SkinRevealConfiguration>();
        
        services.AddFileConfiguration<CharacterSizeModifiersConfiguration>("character_size_modifier/character_size_modifier_configuration");
        services.TryAddSingleton<CharacterSizeModifiersConfiguration>();
        
        services.AddFileConfiguration<FamilyLevelBuffConfiguration>("family_configuration/family_level_buff_configuration");
        services.TryAddSingleton<FamilyLevelBuffConfiguration>();
        
        services.AddFileConfiguration<TattooOptionsConfiguration>("act7/tattoo/tattoo_options");
        services.AddFileConfiguration<CraftTattooItemsConfiguration>("act7/tattoo/craft_tattoo");
        services.AddFileConfiguration<TattooUpgradeConfiguration>("act7/tattoo/tattoo_upgrade");

        services.AddFileConfiguration<CarvedRuneUpgradeConfiguration>("act7/carved_runes/rune_upgrade");
        services.AddFileConfiguration<WeaponRuneCardConfiguration>("act7/carved_runes/weapon_rune_card");
        services.AddFileConfiguration<ArmorRuneCardConfiguration>("act7/carved_runes/armor_rune_card");
        
        services.AddFileConfiguration<LegendaryDropConfiguration>("legendary_drop/legendary_drop_configuration");
        services.TryAddSingleton<LegendaryDropConfiguration>();
        services.AddFileConfiguration<AlzanorConfiguration>("alzanor_event_configuration");
    }
}