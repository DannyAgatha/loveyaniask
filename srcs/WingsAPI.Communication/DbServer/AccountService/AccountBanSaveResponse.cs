using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountBanSaveResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public AccountBanDto AccountBanDto { get; init; }
    }
}