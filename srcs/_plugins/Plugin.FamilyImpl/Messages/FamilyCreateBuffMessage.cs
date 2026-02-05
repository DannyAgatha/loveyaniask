using System;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Packets.Enums;

namespace Plugin.FamilyImpl.Messages;

[MessageType("family.create.buff")]
public class FamilyCreateBuffMessage : IMessage
{
    public long FamilyId { get; set; }
    public string FamilyName { get; set; }
    public int BuffVnum { get; set; }
    public int ItemVnum { get; set; }
    public FactionType? FactionType { get; set; }
    public DateTime EndTime { get; set; }
}