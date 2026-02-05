using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Icebreaker;

[MessageType("game.icebreaker.start")]
public class IcebreakerStartMessage : IMessage
{
    public bool HasNoDelay { get; set; }
}