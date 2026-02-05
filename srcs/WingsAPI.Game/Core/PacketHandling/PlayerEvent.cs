// NosEmu
// 


using PhoenixLib.Events;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game._packetHandling;

public class PlayerEvent : IAsyncEvent
{
    public IClientSession Sender { get; set; }
}