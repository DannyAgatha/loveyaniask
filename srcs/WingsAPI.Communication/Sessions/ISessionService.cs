using System.ServiceModel;
using System.Threading.Tasks;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;

namespace WingsAPI.Communication.Sessions
{
    [ServiceContract]
    public interface ISessionService
    {
        [OperationContract]
        ValueTask<SessionResponse> GetSessionBySessionId(GetSessionBySessionIdRequest request);
        
        [OperationContract]
        ValueTask<SessionResponse> CreateSession(CreateSessionRequest request);

        [OperationContract]
        ValueTask<SessionResponse> GetSessionByAccountName(GetSessionByAccountNameRequest request);

        [OperationContract]
        ValueTask<SessionResponse> GetSessionByAccountId(GetSessionByAccountIdRequest request);

        [OperationContract]
        ValueTask<SessionResponse> ConnectToLoginServer(ConnectToLoginServerRequest request);

        [OperationContract]
        ValueTask<SessionResponse> ConnectToWorldServer(ConnectToWorldServerRequest request);

        [OperationContract]
        ValueTask<SessionResponse> Disconnect(DisconnectSessionRequest request);

        [OperationContract]
        ValueTask<SessionResponse> ConnectCharacter(ConnectCharacterRequest request);

        [OperationContract]
        ValueTask<SessionResponse> ActivateCrossChannelAuthentication(ActivateCrossChannelAuthenticationRequest request);

        [OperationContract]
        ValueTask<SessionResponse> Pulse(PulseRequest request);
    }
}