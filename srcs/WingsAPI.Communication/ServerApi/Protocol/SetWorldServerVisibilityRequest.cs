using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class SetWorldServerVisibilityRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; init; }

        [ProtoMember(2)]
        public string WorldGroup { get; init; }

        /// <summary>
        ///     Being able to see this server requires this AuthorityType or higher
        /// </summary>
        [ProtoMember(3)]
        public AuthorityType AuthorityRequired { get; init; }
    }
}