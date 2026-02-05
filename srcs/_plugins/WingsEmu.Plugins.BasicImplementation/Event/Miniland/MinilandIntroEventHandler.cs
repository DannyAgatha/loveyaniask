using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinilandIntroEventHandler : IAsyncEventProcessor<MinilandIntroEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMinilandManager _minilandManager;

    public MinilandIntroEventHandler(IMinilandManager minilandManager, IGameLanguageService languageService)
    {
        _minilandManager = minilandManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(MinilandIntroEvent e, CancellationToken cancellation)
    {
        e.Sender.PlayerEntity.MinilandMessage = e.RequestedMinilandIntro;

        foreach (IClientSession session in e.Sender.PlayerEntity.Miniland.Sessions)
        {
            if (session.PlayerEntity.Id == e.Sender.PlayerEntity.Id)
            {
                continue;
            }

            session.SendMinilandPublicInformation(_minilandManager, _languageService);
        }

        e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_CHANGED, e.Sender.UserLanguage));
    }
}