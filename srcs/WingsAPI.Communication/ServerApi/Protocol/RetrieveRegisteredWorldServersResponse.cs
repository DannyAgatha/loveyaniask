using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class RetrieveRegisteredWorldServersResponse
    {
        [ProtoMember(1)]
        public List<SerializableGameServer> WorldServers { get; init; }
    }
}