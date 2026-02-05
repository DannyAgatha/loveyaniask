using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Families
{
    [MessageType("family.missions.reset")]
    public class FamilyMissionsResetMessage : IMessage
    {
    }
}