using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.WorldBoss;

[MessageType("worldboss.lottery.daily")]
public class WorldBossDailyLotteryMessage : IMessage
{
    public bool Force { get; init; }
}