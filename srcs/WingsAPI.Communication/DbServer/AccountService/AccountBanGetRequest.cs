using ProtoBuf;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountBanGetRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }
    }
}