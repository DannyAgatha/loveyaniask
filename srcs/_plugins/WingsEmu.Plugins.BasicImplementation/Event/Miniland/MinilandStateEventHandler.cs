using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinilandStateEventHandler : IAsyncEventProcessor<MinilandStateEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMinilandManager _minilandManager;

    public MinilandStateEventHandler(IGameLanguageService languageService, IMinilandManager minilandManager)
    {
        _languageService = languageService;
        _minilandManager = minilandManager;
    }

    public async Task HandleAsync(MinilandStateEvent e, CancellationToken cancellation)
    {
        GameDialogKey gameDialog;
        switch (e.DesiredMinilandState)
        {
            case MinilandState.OPEN:
                gameDialog = GameDialogKey.MINILAND_SHOUTMESSAGE_PUBLIC;
                break;
            case MinilandState.PRIVATE:
                gameDialog = GameDialogKey.MINILAND_SHOUTMESSAGE_PRIVATE;
                KickCharactersFromMiniland(e.Sender);
                break;
            case MinilandState.LOCK:
                gameDialog = GameDialogKey.MINILAND_SHOUTMESSAGE_LOCK;
                KickCharactersFromMiniland(e.Sender, true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        e.Sender.SendMsg(_languageService.GetLanguage(gameDialog, e.Sender.UserLanguage), MsgMessageType.Middle);
        e.Sender.PlayerEntity.MinilandState = e.DesiredMinilandState;
    }

    private void KickCharactersFromMiniland(IClientSession minilandOwner, bool kickFriends = false)
    {
        foreach (IClientSession session in minilandOwner.PlayerEntity.Miniland.Sessions)
        {
            if (session.PlayerEntity.Id == minilandOwner.PlayerEntity.Id)
            {
                continue;
            }

            if (!kickFriends && (session.PlayerEntity.IsFriend(minilandOwner.PlayerEntity.Id) || session.PlayerEntity.IsMarried(minilandOwner.PlayerEntity.Id) || session.IsGameMaster()))
            {
                continue;
            }

            session.ChangeToLastBaseMap();
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED, session.UserLanguage), MsgMessageType.Middle);
        }
    }
}