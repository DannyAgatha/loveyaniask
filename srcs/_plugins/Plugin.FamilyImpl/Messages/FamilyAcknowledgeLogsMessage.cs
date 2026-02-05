using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.acknowledge.logs")]
    public class FamilyAcknowledgeLogsMessage : IMessage
    {
        public IReadOnlyDictionary<long, List<FamilyLogDto>> Logs { get; init; }
    }
}