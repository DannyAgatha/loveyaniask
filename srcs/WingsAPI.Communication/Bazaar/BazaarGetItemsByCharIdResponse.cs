using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Bazaar;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarGetItemsByCharIdResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public ICollection<BazaarItemDTO> BazaarItems { get; set; }
    }
}