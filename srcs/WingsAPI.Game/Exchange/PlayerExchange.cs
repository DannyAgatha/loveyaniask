using System.Collections.Generic;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Exchange;

public class PlayerExchange
{
    public PlayerExchange(long targetId) => TargetId = targetId;

    public long TargetId { get; }
    public IReadOnlyList<ExchangeItem> Items { get; set; }

    public int Gold { get; set; }

    public long BankGold { get; set; }

    public bool RegisteredItems { get; set; }

    public bool AcceptedTrade { get; set; }
}

public class ExchangeItem
{
    public int Amount { get; init; }
    public InventoryType Type { get; init; }
    public short Slot { get; init; }
    public long ItemInstanceId { get; init; }
    public short? Rarity { get; init; }
    public byte? Upgrade { get; init; }
    public string Runes { get; init; }
}