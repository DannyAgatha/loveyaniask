using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game._Guri.Event;

public class GuriEvent : PlayerEvent
{
    public long EffectId { get; set; }

    public int Data { get; set; }

    public long? User { get; set; }

    public string Value { get; set; }
    public string[] Packet { get; set; }
}