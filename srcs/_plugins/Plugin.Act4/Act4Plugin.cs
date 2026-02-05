using Plugin.Act4.Commands;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;

namespace Plugin.Act4;

public class Act4Plugin : IGamePlugin
{
    private readonly ICommandContainer _commands;

    public Act4Plugin(ICommandContainer commands) => _commands = commands;


    public string Name => nameof(Act4Plugin);

    public void OnLoad()
    {
        _commands.AddModule<Act4CommandsModule>();
        _commands.AddModule<Act4CaligorStartCommandModule>();
    }
}