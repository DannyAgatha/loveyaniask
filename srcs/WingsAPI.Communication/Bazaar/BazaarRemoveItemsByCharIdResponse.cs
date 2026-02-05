using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarRemoveItemsByCharIdResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }
    }
}