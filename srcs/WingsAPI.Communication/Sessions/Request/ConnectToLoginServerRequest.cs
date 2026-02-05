using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class ConnectToLoginServerRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }

        [ProtoMember(2)]
        public string HardwareId { get; init; }

        [ProtoMember(3)]
        public string ClientVersion { get; init; }
    }
}