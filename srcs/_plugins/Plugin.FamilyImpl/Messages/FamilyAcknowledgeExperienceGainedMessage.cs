using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.acknowledge.experience_gained")]
    public class FamilyAcknowledgeExperienceGainedMessage : IMessage
    {
        public Dictionary<long, long> Experiences { get; set; }
    }
}