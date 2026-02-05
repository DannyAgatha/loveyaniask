using ProtoBuf;
using WingsAPI.Data.Bazaar;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarItemResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public BazaarItemDTO BazaarItemDto { get; set; }
    }
}