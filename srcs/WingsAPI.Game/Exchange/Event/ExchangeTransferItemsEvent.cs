using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Exchange.Event;

public class ExchangeTransferItemsEvent : PlayerEvent
{
    public IClientSession Target { get; set; }

    public IReadOnlyList<ExchangeItem> SenderItems { get; set; }

    public IReadOnlyList<ExchangeItem> TargetItems { get; set; }

    public int SenderGold { get; set; }

    public long SenderBankGold { get; set; }

    public int TargetGold { get; set; }

    public long TargetBankGold { get; set; }
}

public class ExchangeCompletedEvent : PlayerEvent
{
    public IClientSession Target { get; init; }

    public List<(ItemInstanceDTO, int)> SenderItems { get; init; }

    public List<(ItemInstanceDTO, int)> TargetItems { get; init; }

    public int SenderGold { get; init; }

    public long SenderBankGold { get; init; }

    public int TargetGold { get; init; }

    public long TargetBankGold { get; init; }
}