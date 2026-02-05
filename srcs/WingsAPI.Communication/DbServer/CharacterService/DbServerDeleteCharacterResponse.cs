using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerDeleteCharacterResponse
    {
        [ProtoMember(1)]
        public RpcResponseType RpcResponseType { get; set; }
    }
}