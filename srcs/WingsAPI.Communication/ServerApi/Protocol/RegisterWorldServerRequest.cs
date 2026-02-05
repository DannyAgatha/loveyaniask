using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class RegisterWorldServerRequest
    {
        [ProtoMember(1)]
        public SerializableGameServer GameServer { get; init; }
    }
}