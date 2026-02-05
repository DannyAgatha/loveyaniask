// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.DistributedGameEvents.BotMessages
{
    public class BotMessageConsumer : IMessageConsumer<BotMessageMessage>
    {
        private readonly ISessionManager _sessionManager;

        public BotMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(BotMessageMessage e, CancellationToken cancellation)
        {
            await _sessionManager.BroadcastAsync(async x => x.GenerateMsgPacket(e.Message, MsgMessageType.Middle));
        }
    }
}