// NosEmu
// 


using System;
using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class SerializableGameServer
    {
        [ProtoMember(1)]
        public GameChannelType ChannelType { get; set; }

        [ProtoMember(2)]
        public string WorldGroup { get; set; }

        [ProtoMember(3)]
        public int ChannelId { get; set; }

        [ProtoMember(4)]
        public int AccountLimit { get; set; }

        [ProtoMember(5)]
        public string EndPointIp { get; set; }

        [ProtoMember(6)]
        public int EndPointPort { get; set; }

        [ProtoMember(7)]
        public int SessionCount { get; set; }

        [ProtoMember(8)]
        public AuthorityType Authority { get; set; }

        [ProtoMember(9)]
        public DateTime RegistrationDate { get; set; }
    }
}