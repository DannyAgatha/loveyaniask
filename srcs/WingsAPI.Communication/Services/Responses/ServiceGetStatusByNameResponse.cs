using ProtoBuf;

namespace WingsAPI.Communication.Services.Responses
{
    [ProtoContract]
    public class ServiceGetStatusByNameResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public Service Service { get; init; }
    }
}