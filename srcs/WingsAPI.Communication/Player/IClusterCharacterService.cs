using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Player
{
    [ServiceContract]
    public interface IClusterCharacterService
    {
        [OperationContract]
        ValueTask<ClusterCharacterResponse> GetCharacterById(ClusterCharacterByIdRequest request);

        [OperationContract]
        ValueTask<ClusterCharacterResponse> GetCharacterByName(ClusterCharacterByNameRequest request);

        [OperationContract]
        ValueTask<ClusterCharacterGetMultipleResponse> GetCharactersByChannelId(ClusterCharacterByChannelIdRequest request);

        [OperationContract]
        ValueTask<ClusterCharacterGetSortedResponse> GetCharactersSortedByChannel(EmptyRpcRequest request);

        [OperationContract]
        ValueTask<ClusterCharacterGetMultipleResponse> GetAllCharacters(EmptyRpcRequest request);
    }
}