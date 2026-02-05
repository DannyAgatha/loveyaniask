using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;
using WingsEmu.Game._NpcDialog;

namespace NosEmu.Plugins.BasicImplementations;

public class NpcDialogPluginCore : IGameServerPlugin
{
    public string Name => nameof(NpcDialogPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddHandlers<NpcDialogPluginCore, INpcDialogAsyncHandler>();
        services.AddSingleton<INpcDialogHandlerContainer, NpcDialogHandlerContainer>();
    }
}