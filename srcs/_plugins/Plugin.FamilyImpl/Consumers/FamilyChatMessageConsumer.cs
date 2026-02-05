using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyChatMessageConsumer : IMessageConsumer<FamilyChatMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IServerManager _serverManager;
        private readonly ISessionManager _sessionManager;

        public FamilyChatMessageConsumer(ISessionManager sessionManager, IServerManager serverManager, IGameLanguageService gameLanguage)
        {
            _sessionManager = sessionManager;
            _serverManager = serverManager;
            _gameLanguage = gameLanguage;
        }

        public async Task HandleAsync(FamilyChatMessage e, CancellationToken cancellation)
        {
            string message = $"[{e.SenderNickname}]:{e.Message}";

            if (_serverManager.ChannelId != e.SenderChannelId)
            {
                message = $": {e.SenderChannelId.ToString()}>" + message;
                _sessionManager.BroadcastToFamily(e.SenderFamilyId, x => GetMessage(x, message));
            }
            else
            {
                _sessionManager.BroadcastToFamily(e.SenderFamilyId, x => GenerateFamilyChatLocalChannel(message));
            }
        }

        private static Task<string> GenerateFamilyChatLocalChannel(string message) => Task.FromResult(UiPacketExtension.GenerateSayNoIdPacket(message, ChatMessageColorType.Blue));

        private Task<string> GetMessage(IClientSession session, string message)
            => Task.FromResult(UiPacketExtension.GenerateSayNoIdPacket($"<{_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHANNEL, session.UserLanguage)}" + message, ChatMessageColorType.Blue));
    }
}