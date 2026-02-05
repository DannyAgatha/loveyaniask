using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.RainbowBattle
{
    [MessageType("rainbow-battle.leaver-buster.reset")]
    public class RainbowBattleLeaverBusterResetMessage : IMessage
    {
        public bool Force { get; init; }
    }
}