using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class Act4MukrajuDespawnEventHandler : IAsyncEventProcessor<Act4MukrajuDespawnEvent>
{
    private readonly IAct4Manager _act4Manager;
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public Act4MukrajuDespawnEventHandler(IAct4Manager act4Manager, ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _act4Manager = act4Manager;
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(Act4MukrajuDespawnEvent e, CancellationToken cancellation)
    {
        FactionType mukrajuFaction = _act4Manager.MukrajuFaction();
        IMonsterEntity mukraju = _act4Manager.UnregisterMukraju();
        mukraju.MapInstance.DespawnMonster(mukraju);

        _sessionManager.Broadcast(x =>
        {
            string factionKey = _languageService.GetLanguage(mukrajuFaction == FactionType.Angel ? GameDialogKey.ACT4_SHOUTMESSAGE_CAMP_ANGELS : GameDialogKey.ACT4_SHOUTMESSAGE_CAMP_DEMONS,
                x.UserLanguage);
            return x.GenerateMsgPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_SHOUTMESSAGE_MUKRAJU_DESPAWNED, x.UserLanguage, factionKey), MsgMessageType.Middle);
        });
    }
}