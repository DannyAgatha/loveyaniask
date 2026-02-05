using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Packets.Enums.Bazaar.Filter;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarSearchContext
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [ProtoMember(2)]
        public BazaarCategoryFilterType CategoryFilterType { get; set; }

        [ProtoMember(3)]
        public byte SubTypeFilter { get; set; }

        [ProtoMember(4)]
        public BazaarLevelFilterType LevelFilter { get; set; }

        [ProtoMember(5)]
        public BazaarRarityFilterType RareFilter { get; set; }

        [ProtoMember(6)]
        public BazaarUpgradeFilterType UpgradeFilter { get; set; }

        [ProtoMember(7)]
        public BazaarSortFilterType OrderFilter { get; set; }

        [ProtoMember(8)]
        public IReadOnlyCollection<int> ItemVNumFilter { get; set; }

        [ProtoMember(9)]
        public int AmountOfItemsPerIndex { get; set; }
    }
}