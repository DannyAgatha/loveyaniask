using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Icebreaker;

[MessageType("icebreaker.leaver-buster.reset")]
public class IcebreakerLeaverBusterResetMessage : IMessage
{
    public bool Force { get; init; }
}