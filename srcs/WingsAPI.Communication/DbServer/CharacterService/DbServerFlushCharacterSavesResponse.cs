using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerFlushCharacterSavesResponse
    {
        [ProtoMember(1)]
        public RpcResponseType RpcResponseType { get; set; }
    }
}