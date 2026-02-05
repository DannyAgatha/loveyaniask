using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameStopEventHandler : IAsyncEventProcessor<MinigameStopEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.DeclarateStop;
    private readonly IMinigameManager _minigameManager;

    public MinigameStopEventHandler(IMinigameManager minigameManager) => _minigameManager = minigameManager;

    public Task HandleAsync(MinigameStopEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder lastMinilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

        if (e.MinigameObject != lastMinilandInteraction.MapObject)
        {
            return Task.CompletedTask;
        }

        if (lastMinilandInteraction.Interaction != MinigameInteraction.DeclaratePlay
            && lastMinilandInteraction.Interaction != MinigameInteraction.DeclarateScore
            && lastMinilandInteraction.Interaction != MinigameInteraction.GetReward)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, lastMinilandInteraction.Interaction, lastMinilandInteraction.MapObject, ThisAction, e.MinigameObject);
            return Task.CompletedTask;
        }

        e.Sender.PlayerEntity.CurrentMinigame = 0;
        e.Sender.BroadcastGuri(6, 0, e.Sender.PlayerEntity.Id);
        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MinigameObject));
        return Task.CompletedTask;
    }
}