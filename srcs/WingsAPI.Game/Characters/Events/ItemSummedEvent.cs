using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class ItemSummedEvent : PlayerEvent
{
    public ItemInstanceDTO LeftItem { get; init; }
    public ItemInstanceDTO RightItem { get; init; }
    public bool Succeed { get; init; }
    public int SumLevel { get; init; }
}