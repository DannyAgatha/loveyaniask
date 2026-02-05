using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Mates.PartnerFusion;

namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public class AlgorithmPluginCore : IGameServerPlugin
{
    public string Name => nameof(AlgorithmPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddSingleton<ICharacterAlgorithm, CharacterAlgorithm.CharacterAlgorithm>();
        services.AddSingleton<IPartnerFusionAlgorithm, PartnerFusionAlgorithm>();
    }
}