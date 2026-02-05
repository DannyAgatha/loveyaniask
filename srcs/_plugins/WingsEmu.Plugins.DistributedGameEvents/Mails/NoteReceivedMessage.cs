using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Mails;

namespace WingsEmu.Plugins.DistributedGameEvents.PlayerEvents
{
    [MessageType("note.receive.message")]
    public class NoteReceivedMessage : IMessage
    {
        public CharacterNoteDto SenderNote { get; set; }

        public CharacterNoteDto ReceiverNote { get; set; }
    }
}