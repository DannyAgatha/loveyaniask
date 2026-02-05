using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class CellonUpgradedEvent : PlayerEvent
{
    public ItemInstanceDTO Item { get; init; }
    public int CellonVnum { get; init; }
    public bool Succeed { get; init; }
}