using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.head.sex")]
    public class FamilyHeadSexMessage : IMessage
    {
        public long FamilyId { get; set; }

        public GenderType NewGenderType { get; set; }
    }
}