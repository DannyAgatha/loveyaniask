using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Packets.Enums;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.faction")]
    public class FamilyChangeFactionMessage : IMessage
    {
        public long FamilyId { get; init; }
        public FactionType NewFaction { get; init; }
    }
}