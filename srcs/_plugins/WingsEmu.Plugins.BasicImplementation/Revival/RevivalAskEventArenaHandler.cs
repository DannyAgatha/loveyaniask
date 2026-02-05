using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalAskEventArenaHandler : IAsyncEventProcessor<RevivalAskEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalAskEventArenaHandler(GameRevivalConfiguration gameRevivalConfiguration, IGameLanguageService languageService)
    {
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
        _languageService = languageService;
    }

    public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.AskRevivalType != AskRevivalType.ArenaRevival)
        {
            return;
        }

        string message = _languageService.GetLanguageFormat(GameDialogKey.REVIVE_DIALOG_ARENA, e.Sender.UserLanguage,
            _revivalConfiguration.PlayerRevivalPenalization.ArenaGoldPenalization.ToString());

        e.Sender.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.TryPayArenaRevival),
            CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival), message);
    }
}