using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Player;
using WingsAPI.Data.Character;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class RankingRefreshMessageConsumer : IMessageConsumer<RankingRefreshMessage>
    {
        private readonly IRankingManager _rankingManager;
        private readonly ISessionManager _sessionManager;

        public RankingRefreshMessageConsumer(ISessionManager sessionManager, IRankingManager rankingManager)
        {
            _sessionManager = sessionManager;
            _rankingManager = rankingManager;
        }

        public async Task HandleAsync(RankingRefreshMessage notification, CancellationToken token)
        {
            IReadOnlyList<CharacterDTO> topCompliment = notification.TopCompliment;
            IReadOnlyList<CharacterDTO> topReputation = notification.TopReputation;
            IReadOnlyList<CharacterDTO> topPoints = notification.TopPoints;

            _rankingManager.RefreshRanking(topCompliment, topPoints, topReputation);

            foreach (IClientSession session in _sessionManager.Sessions)
            {
                session.SendClinitPacket(topCompliment);
                session.SendFlinitPacket(topReputation);
                session.SendKdlinitPacket(topPoints);
            }
        }
    }
}