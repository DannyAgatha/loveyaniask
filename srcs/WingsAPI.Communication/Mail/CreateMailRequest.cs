using ProtoBuf;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Mails;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class CreateMailRequest
    {
        [ProtoMember(1)]
        public string SenderName { get; set; }

        [ProtoMember(2)]
        public long ReceiverId { get; set; }

        [ProtoMember(3)]
        public MailGiftType MailGiftType { get; set; }

        [ProtoMember(4)]
        public ItemInstanceDTO ItemInstance { get; set; }
    }
}