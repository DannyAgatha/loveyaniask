using ProtoBuf;
using WingsAPI.Data.Bazaar;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarRemoveItemRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; init; }

        [ProtoMember(2)]
        public BazaarItemDTO BazaarItemDto { get; init; }

        [ProtoMember(3)]
        public long RequesterCharacterId { get; init; }
    }
}