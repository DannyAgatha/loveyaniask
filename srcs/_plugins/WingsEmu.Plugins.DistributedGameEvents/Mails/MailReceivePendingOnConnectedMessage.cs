// NosEmu
// 


using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Mails;

namespace WingsEmu.Plugins.DistributedGameEvents.Mails
{
    /// <summary>
    ///     Limit of 50
    /// </summary>
    [MessageType("mail.connected.message")]
    public class MailReceivePendingOnConnectedMessage : IMessage
    {
        public long CharacterId { get; set; }

        public List<CharacterMailDto> Mails { get; set; }
    }
}