using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerSaveCharactersResponse
    {
        [ProtoMember(1)]
        public RpcResponseType RpcResponseType { get; set; }
    }
}