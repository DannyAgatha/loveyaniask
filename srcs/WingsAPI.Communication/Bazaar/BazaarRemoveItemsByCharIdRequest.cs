using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarRemoveItemsByCharIdRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }
    }
}