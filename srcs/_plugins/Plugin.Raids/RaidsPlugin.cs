using Plugin.Raids.Commands;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;

namespace Plugin.Raids;

public class RaidsPlugin : IGamePlugin
{
    private readonly ICommandContainer _commands;

    public RaidsPlugin(ICommandContainer commands) => _commands = commands;

    public string Name => nameof(RaidsPlugin);

    public void OnLoad()
    {
        _commands.AddModule<RaidAdminCommandsModule>();
        _commands.AddModule<RaidAdminStartModule>();
    }
}