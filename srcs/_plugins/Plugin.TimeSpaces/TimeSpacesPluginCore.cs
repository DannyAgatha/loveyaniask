using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using Plugin.TimeSpaces.RecurrentJob;
using WingsAPI.Plugins;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.Game.TimeSpaces;

namespace Plugin.TimeSpaces;

public class TimeSpacesPluginCore : IGameServerPlugin
{
    public string Name => nameof(TimeSpacesPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddEventHandlersInAssembly<TimeSpacesPluginCore>();
        services.AddSingleton<STimeSpaceValidator>();
        services.AddSingleton<ITimeSpaceScriptManager, LuaTimeSpaceScriptManager>();
        services.AddSingleton<ITimeSpaceFactory, TimeSpaceFactory>();
        services.AddSingleton<ITimeSpaceManager, TimeSpaceManager>();
        services.AddHostedService<TimeSpaceSystem>();
    }
}