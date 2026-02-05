using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Mails;

namespace WingsEmu.Plugins.DistributedGameEvents.Mails
{
    /// <summary>
    ///     Limit of X
    /// </summary>
    [MessageType("note.connected.message")]
    public class NoteReceivePendingOnConnectedMessage : IMessage
    {
        public long CharacterId { get; set; }

        public List<CharacterNoteDto> Notes { get; set; }
    }
}