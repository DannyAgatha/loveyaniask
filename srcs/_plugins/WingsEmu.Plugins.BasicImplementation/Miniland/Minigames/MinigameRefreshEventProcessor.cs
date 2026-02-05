// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Minigames;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Miniland.Minigames;

public class MinigameRefreshProductionEventProcessor : IAsyncEventProcessor<MinigameRefreshProductionEvent>
{
    private readonly IMinigameManager _minilandManager;

    public MinigameRefreshProductionEventProcessor(IMinigameManager minilandManager) => _minilandManager = minilandManager;

    public async Task HandleAsync(MinigameRefreshProductionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        bool canRefresh = await _minilandManager.CanRefreshMinigamesFreeProductionPoints(session.PlayerEntity.Id);

        if (canRefresh == false && e.Force == false)
        {
            session.SendDebugMessage("Miniland points already refreshed for today");
            return;
        }

        if (session.PlayerEntity.MinilandPoint >= 2000)
        {
            session.SendDebugMessage("Miniland points already at maximum free");
            return;
        }

        session.PlayerEntity.MinilandPoint = 2000;
        session.SendInformationChatMessage(session.GetLanguage(GameDialogKey.MINIGAME_CHATMESSAGE_PRODUCTION_REFRESHED));
    }
}