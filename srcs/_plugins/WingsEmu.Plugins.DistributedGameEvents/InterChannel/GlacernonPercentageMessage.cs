using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel;

[MessageType("glacernon.percentage")]
public class GlacernonPercentageMessage : IMessage
{
    public byte AngelPercentage { get; init; }
    public byte DemonPercentage { get; init; }
}