using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountPenaltyMultiSaveResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public IEnumerable<AccountPenaltyDto> AccountPenaltyDtos { get; init; }
    }
}