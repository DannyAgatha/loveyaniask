using ProtoBuf;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class RemoveNoteRequest
    {
        [ProtoMember(1)]
        public long NoteId { get; set; }
    }
}