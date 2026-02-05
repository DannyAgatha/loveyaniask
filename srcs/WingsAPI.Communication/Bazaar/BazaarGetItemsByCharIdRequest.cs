using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ProtoContract]
    public class BazaarGetItemsByCharIdRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }
    }
}