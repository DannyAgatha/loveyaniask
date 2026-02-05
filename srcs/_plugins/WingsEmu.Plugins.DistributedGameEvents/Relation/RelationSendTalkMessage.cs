using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    [MessageType("relation.send.talk")]
    public class RelationSendTalkMessage : IMessage
    {
        public long SenderId { get; set; }
        public long TargetId { get; set; }
        public string Message { get; set; }
    }
}