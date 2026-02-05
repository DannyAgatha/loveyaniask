using ProtoBuf;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class OpenNoteRequest
    {
        [ProtoMember(1)]
        public long NoteId { get; set; }
    }
}