using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Game.Alzanor.Communication;

[MessageType("alzanor.start")]
public class AlzanorStartMessage : IMessage
{
    public bool HasNoDelay { get; set; }
}