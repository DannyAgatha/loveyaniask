using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Mail
{
    [MessageType("logs.mail.removed")]
    public class LogMailRemovedMessage : IPlayerActionLogMessage
    {
        public long MailId { get; set; }
        public string SenderName { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}