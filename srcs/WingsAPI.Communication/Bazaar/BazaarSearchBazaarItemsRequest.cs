using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarSearchBazaarItemsRequest
    {
        [ProtoMember(1)]
        public BazaarSearchContext BazaarSearchContext { get; set; }
    }
}