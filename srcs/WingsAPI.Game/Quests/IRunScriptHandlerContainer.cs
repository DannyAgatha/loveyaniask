using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;

namespace WingsEmu.Game.Quests;

public interface IRunScriptHandlerContainer
{
    Task RegisterAsync(IRunScriptHandler handler);

    Task UnregisterAsync(long runId);

    void Handle(IClientSession player, RunScriptEvent args);

    Task HandleAsync(IClientSession player, RunScriptEvent args);
}