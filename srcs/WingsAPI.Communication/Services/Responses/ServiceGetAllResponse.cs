// NosEmu
// 


using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Communication.Services.Responses
{
    [ProtoContract]
    public class ServiceGetAllResponse
    {
        [ProtoMember(1)]
        public List<Service> Services { get; init; }
    }
}