using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Quests
{
    [MessageType("quest.refresh.daily")]
    public class QuestDailyRefreshMessage : IMessage
    {
        public bool Force { get; init; }
    }
}