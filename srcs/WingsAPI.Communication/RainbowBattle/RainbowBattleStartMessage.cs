using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.RainbowBattle
{
    [MessageType("rainbow-battle.start")]
    public class RainbowBattleStartMessage : IMessage
    {
        public bool HasNoDelay { get; set; }
    }
}