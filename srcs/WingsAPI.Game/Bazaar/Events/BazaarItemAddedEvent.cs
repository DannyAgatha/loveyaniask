using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemAddedEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
    public int Amount { get; init; }
    public DateTime ExpiryDate { get; init; }
    public long PricePerItem { get; init; }
    public long Tax { get; init; }
    public bool UsedMedal { get; init; }
    public bool IsPackage { get; init; }
}