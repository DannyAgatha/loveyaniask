using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateHealEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; set; }
    public int HpHeal { get; set; }
    public int MpHeal { get; set; }
}