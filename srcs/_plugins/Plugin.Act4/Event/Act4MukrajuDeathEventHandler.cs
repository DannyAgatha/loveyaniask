using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class Act4MukrajuDeathEventHandler : IAsyncEventProcessor<Act4MukrajuDeathEvent>
{
    private readonly IAct4Manager _act4Manager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public Act4MukrajuDeathEventHandler(IAct4Manager act4Manager, ISessionManager sessionManager, IGameLanguageService languageService, IAsyncEventPipeline asyncEventPipeline)
    {
        _act4Manager = act4Manager;
        _sessionManager = sessionManager;
        _languageService = languageService;
        _asyncEventPipeline = asyncEventPipeline;
    }

    public async Task HandleAsync(Act4MukrajuDeathEvent e, CancellationToken cancellation)
    {
        FactionType mukrajuFaction = _act4Manager.MukrajuFaction();
        _act4Manager.UnregisterMukraju();

        _sessionManager.Broadcast(x =>
        {
            string factionKey = _languageService.GetLanguage(mukrajuFaction == FactionType.Angel ? GameDialogKey.ACT4_SHOUTMESSAGE_CAMP_ANGELS : GameDialogKey.ACT4_SHOUTMESSAGE_CAMP_DEMONS,
                x.UserLanguage);

            return x.GenerateMsgPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_SHOUTMESSAGE_MUKRAJU_DEATH, x.UserLanguage, factionKey), MsgMessageType.Middle);
        });

        await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonSystemStartEvent(mukrajuFaction), cancellation);
    }
}