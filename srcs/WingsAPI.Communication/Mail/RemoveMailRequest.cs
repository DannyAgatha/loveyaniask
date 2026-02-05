using ProtoBuf;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class RemoveMailRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }

        [ProtoMember(2)]
        public long MailId { get; set; }
    }
}