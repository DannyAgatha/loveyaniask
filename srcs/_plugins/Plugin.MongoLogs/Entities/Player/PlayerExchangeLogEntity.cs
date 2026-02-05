using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Player;
using WingsAPI.Data.Exchanges;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogPlayerExchangeMessage))]
    [CollectionName(CollectionNames.EXCHANGES, DisplayCollectionNames.EXCHANGES)]
    internal class PlayerExchangeLogEntity : IPlayerLogEntity
    {
        public List<LogPlayerExchangeItemInfo> Items { get; set; }
        public long Gold { get; set; }
        public long BankGold { get; set; }
        public long TargetCharacterId { get; set; }

        public List<LogPlayerExchangeItemInfo> TargetItems { get; set; }
        public long TargetGold { get; set; }
        public long TargetBankGold { get; set; }
        public string TargetCharacterName { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}