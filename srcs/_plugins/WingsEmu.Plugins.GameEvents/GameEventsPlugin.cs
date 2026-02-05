using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Plugins.GameEvents.CommandModules;

namespace WingsEmu.Plugins.GameEvents
{
    public class GameEventsPlugin : IGamePlugin
    {
        private readonly ICommandContainer _commandContainer;

        public GameEventsPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

        public string Name => nameof(GameEventsPlugin);

        public void OnLoad()
        {
            _commandContainer.AddModule<GameEventsBasicModule>();
        }
    }
}