using System.Collections.Generic;
using System.Threading.Tasks;
using Master.Managers;
using WingsAPI.Communication;
using WingsAPI.Communication.Player;

namespace WingsEmu.Master
{
    public class ClusterCharacterService : IClusterCharacterService
    {
        private readonly ClusterCharacterManager _clusterCharacterManager;

        public ClusterCharacterService(ClusterCharacterManager clusterCharacterManager) => _clusterCharacterManager = clusterCharacterManager;

        public ValueTask<ClusterCharacterResponse> GetCharacterById(ClusterCharacterByIdRequest request)
        {
            ClusterCharacterInfo info = _clusterCharacterManager.GetCharacterById(request.CharacterId);

            return new ValueTask<ClusterCharacterResponse>(new ClusterCharacterResponse
            {
                ResponseType = info == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                ClusterCharacterInfo = info
            });
        }

        public ValueTask<ClusterCharacterResponse> GetCharacterByName(ClusterCharacterByNameRequest request)
        {
            ClusterCharacterInfo info = _clusterCharacterManager.GetCharacterByName(request.CharacterName);

            return new ValueTask<ClusterCharacterResponse>(new ClusterCharacterResponse
            {
                ResponseType = info == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                ClusterCharacterInfo = info
            });
        }

        public ValueTask<ClusterCharacterGetMultipleResponse> GetCharactersByChannelId(ClusterCharacterByChannelIdRequest request)
        {
            IReadOnlyList<ClusterCharacterInfo> info = _clusterCharacterManager.GetCharactersByChannelId(request.ChannelId);

            return new ValueTask<ClusterCharacterGetMultipleResponse>(new ClusterCharacterGetMultipleResponse
            {
                ResponseType = info == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                ClusterCharacterInfo = info
            });
        }

        public ValueTask<ClusterCharacterGetSortedResponse> GetCharactersSortedByChannel(EmptyRpcRequest request)
        {
            IReadOnlyCollection<KeyValuePair<byte, List<ClusterCharacterInfo>>> info = _clusterCharacterManager.GetCharactersSortedByChannel();

            return new ValueTask<ClusterCharacterGetSortedResponse>(new ClusterCharacterGetSortedResponse
            {
                ResponseType = info == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                CharactersByChannel = info
            });
        }

        public ValueTask<ClusterCharacterGetMultipleResponse> GetAllCharacters(EmptyRpcRequest request)
        {
            IReadOnlyList<ClusterCharacterInfo> info = _clusterCharacterManager.GetCharacters();

            return new ValueTask<ClusterCharacterGetMultipleResponse>(new ClusterCharacterGetMultipleResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                ClusterCharacterInfo = info
            });
        }
    }
}