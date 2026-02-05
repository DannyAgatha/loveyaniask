using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class RemoveAdditionalHpMpEvent : PlayerEvent
{
    public int Hp { get; set; }
    public int Mp { get; set; }
}