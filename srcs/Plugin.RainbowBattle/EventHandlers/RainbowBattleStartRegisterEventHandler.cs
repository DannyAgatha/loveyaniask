using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleStartRegisterEventHandler : IAsyncEventProcessor<RainbowBattleStartRegisterEvent>
    {
        private readonly IRainbowBattleManager _rainbowBattleManager;
        private readonly ISessionManager _sessionManager;

        public RainbowBattleStartRegisterEventHandler(IRainbowBattleManager rainbowBattleManager, ISessionManager sessionManager)
        {
            _rainbowBattleManager = rainbowBattleManager;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(RainbowBattleStartRegisterEvent e, CancellationToken cancellation)
        {
            if (_rainbowBattleManager.IsRegistrationActive)
            {
                return;
            }

            _rainbowBattleManager.RainbowBattleProcessTime = null;
            _rainbowBattleManager.EnableBattleRainbowRegistration();

            _sessionManager.Broadcast(x => x.GenerateEventAsk(QnamlType.RainbowBattle, "guri 503",
                    x.GetLanguageFormat(GameDialogKey.GAMEEVENT_DIALOG_ASK_PARTICIPATE, x.GetLanguage(GameDialogKey.RAINBOW_BATTLE_EVENT_NAME))),
                new InBaseMapBroadcast(), new NotMutedBroadcast());
        }
    }
}