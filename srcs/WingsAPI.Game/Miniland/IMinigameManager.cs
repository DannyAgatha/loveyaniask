using System.Threading.Tasks;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Miniland;

public interface IMinigameManager
{
    Task<bool> CanRefreshMinigamesFreeProductionPoints(long playerEntityId);
    public MinigameScoresHolder GetScores(int minigameVnum);

    public Minigame GetSpecificMinigameConfiguration(int minigameVnum);

    public void RegisterInteraction(IClientSession session, MinilandInteractionInformationHolder minilandInteraction);

    public void ReportInteractionIncoherence(IClientSession session, MinigameInteraction lastInteraction, MapDesignObject lastMapObject,
        MinigameInteraction actualInteraction, MapDesignObject actualMapObject);

    public MinilandInteractionInformationHolder GetLastInteraction(IClientSession session);
}