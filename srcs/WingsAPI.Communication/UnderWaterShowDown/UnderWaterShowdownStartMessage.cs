using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.UnderWaterShowDown;

[MessageType("underwater.showdown.start")]
public class UnderWaterShowdownStartMessage : IMessage;