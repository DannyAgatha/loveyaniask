using ProtoBuf;
using WingsEmu.DTOs.Mails;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class CreateNoteResponse
    {
        [ProtoMember(1)]
        public CharacterNoteDto SenderNote { get; set; }

        [ProtoMember(2)]
        public CharacterNoteDto ReceiverNote { get; set; }

        [ProtoMember(3)]
        public RpcResponseType Status { get; set; }
    }
}