using System;
using ProtoBuf;

namespace WingsAPI.Communication.Services
{
    [ProtoContract]
    public class Service
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public ServiceHealthStatus Status { get; set; }

        [ProtoMember(3)]
        public DateTime LastUpdate { get; set; }
    }
}