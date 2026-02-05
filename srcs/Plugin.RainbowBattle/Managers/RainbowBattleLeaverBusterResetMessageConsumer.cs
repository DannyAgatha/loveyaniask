using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.RainbowBattle;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.Managers
{
    public class RainbowBattleLeaverBusterResetMessageConsumer : IMessageConsumer<RainbowBattleLeaverBusterResetMessage>
    {
        private readonly ISessionManager _sessionManager;

        public RainbowBattleLeaverBusterResetMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(RainbowBattleLeaverBusterResetMessage notification, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;
            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new RainbowBattleLeaverBusterRefreshEvent
                {
                    Force = notification.Force
                });
            }
        }
    }
}