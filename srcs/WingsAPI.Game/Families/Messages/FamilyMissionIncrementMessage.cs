using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Achievements;

[MessageType("family.mission.increment")]
public class FamilyMissionIncrementMessage : IMessage
{
    public long FamilyId { get; set; }
    public long? CharacterId { get; set; }
    public int MissionId { get; set; }
    public int ValueToAdd { get; set; }
}