using System.Collections.Generic;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Exchange.Event;

public class ExchangeRegisterEvent : PlayerEvent
{
    public IReadOnlyList<ExchangeItem> InventoryItems { get; set; }

    public int Gold { get; set; }

    public long BankGold { get; set; }

    public string Packets { get; set; }
}