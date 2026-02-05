using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class CreateSessionRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }

        [ProtoMember(2)]
        public string AccountName { get; init; }

        [ProtoMember(3)]
        public AuthorityType AuthorityType { get; init; }

        [ProtoMember(4)]
        public string IpAddress { get; init; }
    }
}