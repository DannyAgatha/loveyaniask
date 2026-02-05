using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;

namespace NosEmu.Plugins.BasicImplementations;

public class SkillPluginCore : IGameServerPlugin
{
    public string Name => nameof(SkillPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddSkillHandlers();
    }
}