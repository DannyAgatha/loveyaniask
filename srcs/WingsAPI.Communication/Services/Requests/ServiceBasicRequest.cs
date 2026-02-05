using ProtoBuf;

namespace WingsAPI.Communication.Services.Requests
{
    [ProtoContract]
    public class ServiceBasicRequest
    {
        [ProtoMember(1)]
        public string ServiceName { get; init; }
    }
}