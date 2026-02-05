using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyCreatedMessageConsumer : IMessageConsumer<FamilyCreatedMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyCreatedMessageConsumer(ISessionManager sessionManager, IGameLanguageService gameLanguage)
        {
            _sessionManager = sessionManager;
            _gameLanguage = gameLanguage;
        }

        public async Task HandleAsync(FamilyCreatedMessage e, CancellationToken cancellation)
        {
            await _sessionManager.BroadcastAsync(async s =>
                s.GenerateMsgPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.FAMILY_SHOUTMESSAGE_FAMILY_CREATED, s.UserLanguage, e.FamilyName), MsgMessageType.Middle));
        }
    }
}