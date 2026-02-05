using System;
using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace WingsAPI.Communication.Sessions.Model
{
    [ProtoContract]
    public class Session
    {
        [ProtoMember(1)]
        public string Id { get; init; }

        [ProtoMember(2)]
        public AuthorityType Authority { get; init; }

        [ProtoMember(3)]
        public long AccountId { get; init; }

        [ProtoMember(4)]
        public string IpAddress { get; init; }

        [ProtoMember(5)]
        public string AccountName { get; init; }

        [ProtoMember(6)]
        public SessionState? State { get; set; }

        [ProtoMember(7)]
        public string HardwareId { get; set; }

        [ProtoMember(8)]
        public string ClientVersion { get; set; }

        [ProtoMember(9)]
        public int EncryptionKey { get; set; }

        [ProtoMember(10)]
        public DateTime LastPulse { get; set; }

        [ProtoMember(11)]
        public long CharacterId { get; set; }

        [ProtoMember(12)]
        public int ChannelId { get; set; }

        [ProtoMember(13)]
        public int LastChannelId { get; set; }

        [ProtoMember(14)]
        public string ServerGroup { get; set; }

        [ProtoMember(15)]
        public long AllowedCrossChannelId { get; set; }
    }
}