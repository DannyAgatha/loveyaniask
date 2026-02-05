// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;

namespace NosEmu.Plugins.BasicImplementations.Event.AntiCheat;

public class GameMasterNotifierStrangeBehaviorEventHandler : IAsyncEventProcessor<StrangeBehaviorEvent>
{
    private readonly ISessionManager _sessionManager;

    public GameMasterNotifierStrangeBehaviorEventHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(StrangeBehaviorEvent e, CancellationToken cancellation) =>
        _sessionManager.BroadcastToGameMaster($"[STRANGE_BEHAVIOR][{e.Severity}] {e.Sender?.PlayerEntity?.Name ?? $"[NOT_INITIALIZED Account: {e.Sender?.Account?.Id}]"} => {e.Reason}");
}