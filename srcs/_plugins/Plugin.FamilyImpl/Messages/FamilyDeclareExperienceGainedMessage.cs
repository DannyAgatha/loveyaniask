using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.declare.experience_gained")]
    public class FamilyDeclareExperienceGainedMessage : IMessage
    {
        public IReadOnlyCollection<ExperienceGainedSubMessage> Experiences { get; set; }
    }
}