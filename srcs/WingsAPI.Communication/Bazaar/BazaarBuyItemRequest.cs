using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarBuyItemRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; set; }

        [ProtoMember(2)]
        public long BazaarItemId { get; set; }

        [ProtoMember(3)]
        public long BuyerCharacterId { get; set; }

        [ProtoMember(4)]
        public int Amount { get; set; }

        [ProtoMember(5)]
        public long PricePerItem { get; set; }
    }
}