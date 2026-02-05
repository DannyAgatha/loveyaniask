using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class ShutdownRequest
    {
        [ProtoMember(1)]
        public string WorldGroup { get; init; }
    }
}