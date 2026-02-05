using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarGetItemByIdRequest
    {
        [ProtoMember(1)]
        public long BazaarItemId { get; init; }
    }
}