using ProtoBuf;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountLoadByNameRequest
    {
        [ProtoMember(1)]
        public string Name { get; init; }
    }
}