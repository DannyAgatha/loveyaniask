using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;

namespace Plugin.QuestImpl
{
    public class RunScriptEventHandler : IAsyncEventProcessor<RunScriptEvent>
    {
        private readonly IRunScriptHandlerContainer _runScriptHandler;

        public RunScriptEventHandler(IRunScriptHandlerContainer runScriptHandler) => _runScriptHandler = runScriptHandler;

        public async Task HandleAsync(RunScriptEvent e, CancellationToken cancellation)
        {
            await Task.Run(() => _runScriptHandler.Handle(e.Sender, e), cancellation);
        }
    }
}