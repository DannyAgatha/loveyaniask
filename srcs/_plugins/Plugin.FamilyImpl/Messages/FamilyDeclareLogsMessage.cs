using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.declare.logs")]
    public class FamilyDeclareLogsMessage : IMessage
    {
        public IReadOnlyList<FamilyLogDto> Logs { get; set; }
    }
}