using ProtoBuf;
using WingsAPI.Data.Bazaar;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarChangeItemPriceRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; set; }

        [ProtoMember(2)]
        public BazaarItemDTO BazaarItemDto { get; set; }

        [ProtoMember(3)]
        public long ChangerCharacterId { get; init; }

        [ProtoMember(4)]
        public long NewPrice { get; set; }

        [ProtoMember(5)]
        public long NewSaleFee { get; set; }
    }
}