using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalAskEventHandler : IAsyncEventProcessor<RevivalAskEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalAskEventHandler(GameRevivalConfiguration gameRevivalConfiguration, IGameLanguageService languageService)
    {
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
        _languageService = languageService;
    }

    public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.AskRevivalType != AskRevivalType.BasicRevival)
        {
            return;
        }

        PlayerRevivalPenalization playerRevivalPenalization = _revivalConfiguration.PlayerRevivalPenalization;
        string message = e.Sender.PlayerEntity.Level > playerRevivalPenalization.MaxLevelWithoutRevivalPenalization
            ? _languageService.GetLanguageFormat(GameDialogKey.REVIVE_DIALOG_SEEDS_OF_POWER, e.Sender.UserLanguage, playerRevivalPenalization.BaseMapRevivalPenalizationSaverAmount)
            : _languageService.GetLanguageFormat(GameDialogKey.REVIVE_DIALOG_FREE, e.Sender.UserLanguage, playerRevivalPenalization.MaxLevelWithoutRevivalPenalization);

        e.Sender.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.TryPayRevival), CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival),
            message);
    }
}