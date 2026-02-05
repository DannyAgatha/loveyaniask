using System.ServiceModel;
using System.Threading.Tasks;
using WingsAPI.Communication.ServerApi.Protocol;

namespace WingsAPI.Communication.ServerApi
{
    [ServiceContract]
    public interface IServerApiService
    {
        [OperationContract]
        ValueTask<BasicRpcResponse> IsMasterOnline(EmptyRpcRequest request);

        /*
         * World
         */
        [OperationContract]
        ValueTask<BasicRpcResponse> RegisterWorldServer(RegisterWorldServerRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> PulseWorldServer(PulseWorldServerRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> UnregisterWorldServer(UnregisterWorldServerRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> SetWorldServerVisibility(SetWorldServerVisibilityRequest request);

        [OperationContract]
        ValueTask<RetrieveRegisteredWorldServersResponse> RetrieveRegisteredWorldServers(RetrieveRegisteredWorldServersRequest request);

        [OperationContract]
        Task<RetrieveRegisteredWorldServersResponse> RetrieveAllGameServers(EmptyRpcRequest request);

        [OperationContract]
        ValueTask<GetChannelInfoResponse> GetChannelInfo(GetChannelInfoRequest request);

        [OperationContract]
        ValueTask<GetChannelInfoResponse> GetAct4ChannelInfo(GetAct4ChannelInfoRequest request);
    }
}