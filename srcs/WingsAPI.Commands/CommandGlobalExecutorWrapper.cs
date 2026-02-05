// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game.Commands;
using WingsEmu.Game.Networking;

namespace WingsEmu.Commands
{
    public class CommandGlobalExecutorWrapper : IGlobalCommandExecutor
    {
        private readonly ICommandContainer _commandContainer;

        public CommandGlobalExecutorWrapper(ICommandContainer commandContainer) => _commandContainer = commandContainer;

        public void HandleCommand(string command, IClientSession sender, string prefix)
        {
            HandleCommandAsync(command, sender, prefix).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task HandleCommandAsync(string command, IClientSession sender, string prefix)
        {
            await _commandContainer.HandleMessageAsync(command, sender, prefix);
        }
    }
}