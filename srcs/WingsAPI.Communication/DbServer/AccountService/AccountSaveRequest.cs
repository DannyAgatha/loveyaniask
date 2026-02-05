using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountSaveRequest
    {
        [ProtoMember(1)]
        public AccountDTO AccountDto { get; init; }
    }
}