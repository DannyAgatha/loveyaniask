using System;
using System.Collections.Generic;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Exchanges;

namespace Plugin.PlayerLogs.Messages.Player
{
    [MessageType("logs.trade.complete")]
    public class LogPlayerExchangeMessage : IPlayerActionLogMessage
    {
        public List<LogPlayerExchangeItemInfo> Items { get; set; }
        public long Gold { get; set; }
        public long BankGold { get; set; }
        public long TargetCharacterId { get; set; }
        public List<LogPlayerExchangeItemInfo> TargetItems { get; set; }
        public long TargetGold { get; set; }
        public long TargetBankGold { get; set; }
        public string TargetCharacterName { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}