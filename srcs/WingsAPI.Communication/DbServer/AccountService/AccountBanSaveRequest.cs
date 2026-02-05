using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountBanSaveRequest
    {
        [ProtoMember(1)]
        public AccountBanDto AccountBanDto { get; init; }
    }
}