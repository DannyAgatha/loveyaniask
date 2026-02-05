using ProtoBuf;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class GetMailsRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }
    }
}