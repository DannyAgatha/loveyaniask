using System.Collections.Generic;
using WingsAPI.Packets.Enums.Bazaar.Filter;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarSearchItemsEvent : PlayerEvent
{
    public BazaarSearchItemsEvent(int index, BazaarCategoryFilterType categoryFilterType, byte subTypeFilter, BazaarLevelFilterType levelFilter, BazaarRarityFilterType rareFilter,
        BazaarUpgradeFilterType upgradeFilter, BazaarSortFilterType orderFilter, IReadOnlyCollection<int> itemVNumFilter)
    {
        Index = index;
        CategoryFilterType = categoryFilterType;
        SubTypeFilter = subTypeFilter;
        LevelFilter = levelFilter;
        RareFilter = rareFilter;
        UpgradeFilter = upgradeFilter;
        OrderFilter = orderFilter;
        ItemVNumFilter = itemVNumFilter;
    }

    public int Index { get; }

    public BazaarCategoryFilterType CategoryFilterType { get; }

    public byte SubTypeFilter { get; }

    public BazaarLevelFilterType LevelFilter { get; }

    public BazaarRarityFilterType RareFilter { get; }

    public BazaarUpgradeFilterType UpgradeFilter { get; }

    public BazaarSortFilterType OrderFilter { get; }

    public IReadOnlyCollection<int> ItemVNumFilter { get; }
}