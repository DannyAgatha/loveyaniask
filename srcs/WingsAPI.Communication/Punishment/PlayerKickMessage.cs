using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Punishment
{
    [MessageType("player.kick")]
    public class PlayerKickMessage : IMessage
    {
        public long? PlayerId { get; init; }
        public string PlayerName { get; init; }
    }
}