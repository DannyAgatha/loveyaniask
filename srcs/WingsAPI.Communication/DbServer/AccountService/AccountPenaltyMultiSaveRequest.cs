using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountPenaltyMultiSaveRequest
    {
        [ProtoMember(1)]
        public IReadOnlyList<AccountPenaltyDto> AccountPenaltyDtos { get; init; }
    }
}