// NosEmu
// 


using WingsAPI.Packets.Enums.Bazaar.Filter;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("c_blist")]
    public class CbListPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int Index { get; set; }

        [PacketIndex(1)]
        public BazaarCategoryFilterType CategoryFilterType { get; set; }

        [PacketIndex(2)]
        public byte SubTypeFilter { get; set; }

        [PacketIndex(3)]
        public BazaarLevelFilterType LevelFilter { get; set; }

        [PacketIndex(4)]
        public BazaarRarityFilterType RareFilter { get; set; }

        [PacketIndex(5)]
        public BazaarUpgradeFilterType UpgradeFilter { get; set; }

        [PacketIndex(6)]
        public BazaarSortFilterType OrderFilter { get; set; }

        [PacketIndex(7, serializeToEnd: true)]
        public string ItemVNumFilter { get; set; }

        #endregion
    }
}