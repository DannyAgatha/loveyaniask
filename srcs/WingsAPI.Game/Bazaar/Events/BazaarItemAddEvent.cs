using System;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemAddEvent : PlayerEvent
{
    public InventoryItem InventoryItem { get; init; }

    public int Amount { get; init; }

    public DateTime ExpiryDate { get; init; }

    public short DayExpiryAmount { get; init; }

    public long PricePerItem { get; init; }

    public long Tax { get; init; }

    public bool UsedMedal { get; init; }

    public bool IsPackage { get; init; }
}