using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Mails;

namespace WingsEmu.Plugins.DistributedGameEvents.Mails
{
    [MessageType("mail.receive.message")]
    public class MailReceivedMessage : IMessage
    {
        public long CharacterId { get; set; }

        public IEnumerable<CharacterMailDto> MailDtos { get; set; }
    }
}