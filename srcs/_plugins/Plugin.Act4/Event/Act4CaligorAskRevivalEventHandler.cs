using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Revival;

namespace Plugin.Act4.Event
{
    public class Act4CaligorAskRevivalEventHandler : IAsyncEventProcessor<RevivalAskEvent>
    {
        private readonly IGameLanguageService _languageService;

        public Act4CaligorAskRevivalEventHandler(IGameLanguageService languageService)
        {
            _languageService = languageService;
        }

        public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
        {
            if (e.Sender.PlayerEntity.IsAlive() || e.AskRevivalType != AskRevivalType.CaligorRevival)
            {
                return;
            }

            e.Sender.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.TryPayRevival), CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival),
                _languageService.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_REVIVAL_DIALOG, e.Sender.UserLanguage));
        }
    }
}