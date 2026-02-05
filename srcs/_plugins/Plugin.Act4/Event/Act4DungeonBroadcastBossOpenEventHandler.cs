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

public class Act4DungeonBroadcastBossOpenEventHandler : IAsyncEventProcessor<Act4DungeonBroadcastBossOpenEvent>
{
    private readonly IGameLanguageService _languageService;

    public Act4DungeonBroadcastBossOpenEventHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(Act4DungeonBroadcastBossOpenEvent e, CancellationToken cancellation)
    {
        foreach (DungeonSubInstance subInstance in e.DungeonInstance.DungeonSubInstances.Values)
        {
            if (subInstance.MapInstance.Sessions.Count < 1)
            {
                continue;
            }

            foreach (IClientSession session in subInstance.MapInstance.Sessions)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.ACT4_DUNGEON_SHOUTMESSAGE_BOSS_OPEN, session.UserLanguage), MsgMessageType.Middle);
            }
        }
    }
}