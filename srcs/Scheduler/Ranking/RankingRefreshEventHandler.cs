using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Player;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;

namespace WingsEmu.ClusterScheduler.Ranking
{
    public class RankingRefreshEventHandler : IAsyncEventProcessor<RankingRefreshEvent>
    {
        private readonly ICharacterService _characterService;
        private readonly IMessagePublisher<RankingRefreshMessage> _message;

        public RankingRefreshEventHandler(IMessagePublisher<RankingRefreshMessage> message, ICharacterService characterService)
        {
            _message = message;
            _characterService = characterService;
        }

        public async Task HandleAsync(RankingRefreshEvent e, CancellationToken cancellation)
        {
            CharacterRefreshRankingResponse response = null;

            try
            {
                response = await _characterService.RefreshRanking(new EmptyRpcRequest());
            }
            catch (Exception ex)
            {
                Log.Error("[RANKING_REFRESH] Unexpected error: ", ex);
            }

            if (response?.ResponseType != RpcResponseType.SUCCESS)
            {
                Log.Warn("[RANKING_REFRESH] Issue while trying to refresh the Ranking");
                return;
            }

            await _message.PublishAsync(new RankingRefreshMessage
            {
                TopCompliment = response.TopCompliment ?? new List<CharacterDTO>(),
                TopPoints = response.TopPoints ?? new List<CharacterDTO>(),
                TopReputation = response.TopReputation ?? new List<CharacterDTO>(),
                GlobalFamily = response.GlobalFamily ?? new List<FamilyDTO>()
            });
        }
    }
}