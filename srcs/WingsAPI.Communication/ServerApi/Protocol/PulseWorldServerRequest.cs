using System;
using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class PulseWorldServerRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; init; }

        [ProtoMember(2)]
        public int SessionsCount { get; init; }

        [ProtoMember(3)]
        public DateTime StartDate { get; init; }
    }
}