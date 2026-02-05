using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountSaveResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public AccountDTO AccountDto { get; init; }
    }
}