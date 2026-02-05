using ProtoBuf;
using WingsAPI.Communication.Sessions.Model;

namespace WingsAPI.Communication.Sessions.Response
{
    [ProtoContract]
    public class SessionResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public Session Session { get; init; }
    }
}