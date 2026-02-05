using ProtoBuf;
using WingsAPI.Data.Bazaar;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarAddItemRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; set; }

        [ProtoMember(2)]
        public BazaarItemDTO BazaarItemDto { get; set; }

        [ProtoMember(4)]
        public string OwnerName { get; set; }

        [ProtoMember(5)]
        public long SunkGold { get; set; }

        [ProtoMember(6)]
        public int MaximumListedItems { get; set; }
    }
}