using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountPenaltyGetAllResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public List<AccountPenaltyDto> AccountPenaltyDtos { get; init; }
    }
}