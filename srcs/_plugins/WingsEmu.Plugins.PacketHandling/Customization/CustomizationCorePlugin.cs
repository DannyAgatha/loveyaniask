// NosEmu
// 


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Configuration;
using WingsAPI.Plugins;
using WingsEmu.Customization.NewCharCustomisation;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using NosEmu.Plugins.BasicImplementations.Warehouse;
using WingsEmu.Game.Configurations.InitialConfiguration;
using DependencyInjectionExtensions = NosEmu.Plugins.BasicImplementations.Miniland.DependencyInjectionExtensions;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class CustomizationCorePlugin : IGameServerPlugin
{
    public string Name => nameof(CustomizationCorePlugin);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddFileConfiguration(new BaseCharacter());
        services.AddFileConfiguration<BaseQuicklist>();
        services.AddFileConfiguration<BaseInventory>();
        services.AddFileConfiguration<BaseSkill>();
        
        services.AddSingleton<BaseInventorySwordman>();
        services.AddFileConfiguration<BaseSkillSwordman>();
        services.AddFileConfiguration<BaseQuickListSwordman>();

        services.AddSingleton<BaseInventoryArcher>();
        services.AddFileConfiguration<BaseSkillArcher>();
        services.AddFileConfiguration<BaseQuickListArcher>();

        services.AddSingleton<BaseInventoryMagician>();
        services.AddFileConfiguration<BaseSkillMagician>();
        services.AddFileConfiguration<BaseQuickListMagician>();

        services.AddSingleton<BaseInventoryMartialArtist>();
        services.AddFileConfiguration<BaseSkillMartialArtist>();
        services.AddFileConfiguration<BaseQuickListMartialArtist>();
        
        services.AddFileConfiguration<GeneralServerConfiguration>();
        services.AddFileConfiguration<GameRateConfiguration>();
        services.AddFileConfiguration<GameMinMaxConfiguration>();
        services.AddFileConfiguration<GameRevivalConfiguration>();
        services.AddFileConfiguration<SpPerfectionConfiguration>();
        services.AddFileConfiguration<SpUpgradeConfiguration>();
        services.AddFileConfiguration<EvtbFileConfiguration>("evtb/evtb_configuration");
        services.TryAddSingleton<IEvtbConfiguration, EvtbConfiguration>();
        services.AddFileConfiguration<GeneralEvtbFile>("evtb/automatic_evtb_configuration");
        services.AddFileConfiguration<BankReputationInfo>("bank_reputation_configuration");
        services.TryAddSingleton<IBankReputationConfiguration, BankReputationConfiguration>();
        services.AddMultipleConfigurationOneFile<ReputationInfo>("reputation_configuration");
        services.TryAddSingleton<IReputationConfiguration, ReputationConfiguration>();
        services.AddMultipleConfigurationOneFile<BuffDuration>("buffs_duration_configuration");
        services.TryAddSingleton<IBuffsDurationConfiguration, BuffsDurationConfiguration>();
        services.AddMultipleConfigurationOneFile<RespawnDefault>("return_default_configuration");
        services.TryAddSingleton<IRespawnDefaultConfiguration, RespawnDefaultConfiguration>();
        services.AddFileConfiguration<SpecialistSkinConfiguration>();
        DependencyInjectionExtensions.AddMinilandModule(services);
        services.AddWarehouseModule();
    }
}