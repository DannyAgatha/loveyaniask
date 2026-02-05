using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class DisconnectSessionRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }

        [ProtoMember(2)]
        public long EncryptionKey { get; init; }

        [ProtoMember(3)]
        public bool ForceDisconnect { get; init; }
    }
}