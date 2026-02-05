using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameGetYieldInfoEventHandler : IAsyncEventProcessor<MinigameGetYieldInfoEvent>
{
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;
    private readonly MinigameInteraction ThisAction = MinigameInteraction.GetYieldInformation;

    public MinigameGetYieldInfoEventHandler(IMinigameManager minigameManager, MinigameConfiguration minigameConfiguration)
    {
        _minigameManager = minigameManager;
        _minigameConfiguration = minigameConfiguration;
    }

    public async Task HandleAsync(MinigameGetYieldInfoEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder minilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

        if (minilandInteraction.Interaction != MinigameInteraction.GetMinigameInformation
            && minilandInteraction.Interaction != MinigameInteraction.GetMinigameDurability
            && minilandInteraction.Interaction != MinigameInteraction.RepairMinigameDurability
            && minilandInteraction.MapObject != e.MapObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, minilandInteraction.Interaction, minilandInteraction.MapObject, ThisAction, e.MapObject);
            return;
        }

        if (e.MapObject.CharacterId != e.Sender.PlayerEntity.Id)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to see information about the yield rewards from a minigame that he doesn't own." +
                $" 'SuspectCharacterId': {e.Sender.PlayerEntity.Id.ToString()} | 'VictimCharacterId': {e.MapObject.CharacterId.ToString()}");
            return;
        }

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject));
        e.Sender.SendMinilandYieldInfo(e.MapObject, e.MapObject.GetYieldRewardEnumerable(), _minigameConfiguration);
    }
}