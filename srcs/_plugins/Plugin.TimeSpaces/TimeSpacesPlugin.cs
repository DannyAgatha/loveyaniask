using Plugin.TimeSpaces.Commands;
using WingsAPI.Plugins;
using WingsAPI.Scripting;
using WingsEmu.Commands.Interfaces;

namespace Plugin.TimeSpaces;

public class TimeSpacesPlugin : IGamePlugin
{
    private readonly ICommandContainer _commands;
    private readonly IScriptFactory _scriptFactory;

    public TimeSpacesPlugin(ICommandContainer commands, IScriptFactory scriptFactory)
    {
        _commands = commands;
        _scriptFactory = scriptFactory;
    }

    public string Name { get; } = nameof(TimeSpacesPlugin);

    public void OnLoad()
    {
        _commands.AddModule<TimeSpaceAdminStartModule>();

        _scriptFactory.RegisterAllScriptingObjectsInAssembly(typeof(TimeSpacesPlugin).Assembly);
    }
}