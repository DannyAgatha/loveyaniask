using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class ItemGambledEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
    public RarifyMode Mode { get; init; }
    public RarifyProtection Protection { get; init; }
    public int? Amulet { get; init; }
    public bool Succeed { get; init; }
    public short OriginalRarity { get; init; }
    public short? FinalRarity { get; init; }
}