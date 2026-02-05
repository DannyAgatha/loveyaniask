using ProtoBuf;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountLoadByIdRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }
    }
}