using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Arena;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Arena;

public class JoinArenaHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly ISessionManager _sessionManager;

    public JoinArenaHandler(IGameLanguageService langService, ISessionManager sessionManager)
    {
        _langService = langService;
        _sessionManager = sessionManager;
    }

    public long[] HandledIds => new long[] { 17 };

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ASK_ENTER_ARENA };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        double timeSpanSinceLastPortal = (DateTime.UtcNow - session.PlayerEntity.LastPortal).TotalSeconds;

        if (timeSpanSinceLastPortal < 4 || !session.HasCurrentMapInstance)
        {
            session.SendChatMessage(_langService.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_TOO_EARLY, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        bool asksForFamilyArena = e.Argument != 0;

        if (session.CantPerformActionOnAct4() || !session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) || !session.PlayerEntity.IsAlive())
        {
            return;
        }

        long cost = ArenaExtensions.GetArenaEntryPrice(asksForFamilyArena);

        session.SendQnaPacket($"arena {(asksForFamilyArena ? 1 : 0).ToString()}",
            _langService.GetLanguageFormat(asksForFamilyArena ? GameDialogKey.FAMILYARENA_INFO_ASK_ENTER : GameDialogKey.ARENA_INFO_ASK_ENTER, session.UserLanguage, cost.ToString()));
    }
}