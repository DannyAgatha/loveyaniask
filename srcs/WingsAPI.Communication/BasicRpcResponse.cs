using ProtoBuf;

namespace WingsAPI.Communication
{
    [ProtoContract]
    public class BasicRpcResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }
    }
}