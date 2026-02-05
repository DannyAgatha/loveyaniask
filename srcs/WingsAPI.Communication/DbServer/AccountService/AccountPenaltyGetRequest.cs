using ProtoBuf;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountPenaltyGetRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }
    }
}