using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class RetrieveRegisteredWorldServersRequest
    {
        [ProtoMember(1)]
        public AuthorityType RequesterAuthority { get; init; }
    }
}