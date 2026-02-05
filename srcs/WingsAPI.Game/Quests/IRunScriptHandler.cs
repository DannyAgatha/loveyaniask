using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;

namespace WingsEmu.Game.Quests;

public interface IRunScriptHandler
{
    public int[] RunIds { get; }

    Task ExecuteAsync(IClientSession session, RunScriptEvent e);
}