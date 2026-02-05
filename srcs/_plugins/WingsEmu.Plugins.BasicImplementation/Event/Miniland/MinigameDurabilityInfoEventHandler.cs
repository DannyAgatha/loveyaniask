using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameDurabilityInfoEventHandler : IAsyncEventProcessor<MinigameDurabilityInfoEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.GetMinigameDurability;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;

    public MinigameDurabilityInfoEventHandler(MinigameConfiguration minigameConfiguration, IMinigameManager minigameManager)
    {
        _minigameConfiguration = minigameConfiguration;
        _minigameManager = minigameManager;
    }

    public async Task HandleAsync(MinigameDurabilityInfoEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder lastMinilandInteraction = _minigameManager.GetLastInteraction(e.Sender);
        if (lastMinilandInteraction.Interaction != MinigameInteraction.GetMinigameInformation
            && lastMinilandInteraction.Interaction != MinigameInteraction.RepairMinigameDurability
            && lastMinilandInteraction.Interaction != MinigameInteraction.GetYieldInformation
            && lastMinilandInteraction.Interaction != MinigameInteraction.GetYieldReward
            && lastMinilandInteraction.MapObject != e.MapObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, lastMinilandInteraction.Interaction, lastMinilandInteraction.MapObject, ThisAction, e.MapObject);
            return;
        }

        if (e.MapObject.CharacterId != e.Sender.PlayerEntity.Id)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to see durability information from a minigame that he doesn't own." +
                $" 'SuspectCharacterId': {e.Sender.PlayerEntity.Id.ToString()} | 'VictimCharacterId': {e.MapObject.CharacterId.ToString()}");
            return;
        }

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject));
        e.Sender.SendMinilandDurabilityInfo(e.MapObject, _minigameConfiguration);
    }
}