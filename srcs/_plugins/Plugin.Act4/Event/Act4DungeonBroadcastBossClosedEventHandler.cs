using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class Act4DungeonBroadcastBossClosedEventHandler : IAsyncEventProcessor<Act4DungeonBroadcastBossClosedEvent>
{
    private readonly IGameLanguageService _languageService;

    public Act4DungeonBroadcastBossClosedEventHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(Act4DungeonBroadcastBossClosedEvent e, CancellationToken cancellation)
    {
        DungeonInstance dungeonInstance = e.DungeonInstanceWrapper.DungeonInstance;

        foreach (DungeonSubInstance dungeonSubInstance in dungeonInstance.DungeonSubInstances.Values)
        {
            foreach (IClientSession session in dungeonSubInstance.MapInstance.Sessions)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.ACT4_DUNGEON_SHOUTMESSAGE_BOSS_CLOSED, session.UserLanguage), MsgMessageType.Middle);
            }
        }
    }
}