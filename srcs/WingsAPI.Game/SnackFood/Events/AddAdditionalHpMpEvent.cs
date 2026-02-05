using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.SnackFood.Events;

public class AddAdditionalHpMpEvent : PlayerEvent
{
    public int Hp { get; init; }
    public int Mp { get; init; }

    public int MaxHpPercentage { get; set; }
    public int MaxMpPercentage { get; set; }
}