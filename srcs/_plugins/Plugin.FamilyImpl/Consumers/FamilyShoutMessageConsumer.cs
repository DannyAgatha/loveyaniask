using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyShoutMessageConsumer : IMessageConsumer<FamilyShoutMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyShoutMessageConsumer(ISessionManager sessionManager, IGameLanguageService gameLanguage)
        {
            _sessionManager = sessionManager;
            _gameLanguage = gameLanguage;
        }

        public async Task HandleAsync(FamilyShoutMessage notification, CancellationToken token)
        {
            string message = notification.Message;
            long familyId = notification.FamilyId;
            GameDialogKey gameDialogKey = notification.GameDialogKey;

            await _sessionManager.BroadcastAsync(async s =>
            {
                string msg = _gameLanguage.GetLanguageFormat(gameDialogKey, s.UserLanguage, notification.SenderName, message);
                return s.GenerateMsgPacket(msg, MsgMessageType.Middle);
            }, new FamilyBroadcast(familyId));
        }
    }
}