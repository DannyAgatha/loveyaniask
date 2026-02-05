using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Rewards;

[MessageType("dailyrewards.restriction-refresh")]
public class RewardRestrictionRefreshMessage : IMessage
{
}