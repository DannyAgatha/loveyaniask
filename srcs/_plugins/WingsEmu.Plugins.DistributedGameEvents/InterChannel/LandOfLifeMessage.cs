using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel;

[MessageType("land.of.life.init.message")]
public class LandOfLifeMessage : IMessage
{
    public string Message { get; init; }
}