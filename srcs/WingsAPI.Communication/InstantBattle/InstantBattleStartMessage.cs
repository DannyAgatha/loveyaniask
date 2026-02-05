// NosEmu
// 


using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.InstantBattle
{
    [MessageType("game.instant-battle.start")]
    public class InstantBattleStartMessage : IMessage
    {
        public bool HasNoDelay { get; set; }
    }
}