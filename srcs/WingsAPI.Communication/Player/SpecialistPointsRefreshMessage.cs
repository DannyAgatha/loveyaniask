using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Player
{
    [MessageType("specialist.points.refresh")]
    public class SpecialistPointsRefreshMessage : IMessage
    {
        public bool Force { get; set; }
    }
}