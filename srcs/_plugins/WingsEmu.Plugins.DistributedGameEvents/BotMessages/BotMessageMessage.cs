// NosEmu
// 


using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.BotMessages
{
    [MessageType("game.bot.broadcast-message")]
    public class BotMessageMessage : IMessage
    {
        public string Message { get; set; }
    }
}