using Plugin.Act6.Commands;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;

namespace Plugin.Act6
{
    public class Act6Plugin : IGamePlugin
    {
        private readonly ICommandContainer _commands;

        public Act6Plugin(ICommandContainer commands) => _commands = commands;

        public string Name => nameof(Act6Plugin);

        public void OnLoad()
        {
            _commands.AddModule<Act6CommandsModule>();
        }
    }
}
