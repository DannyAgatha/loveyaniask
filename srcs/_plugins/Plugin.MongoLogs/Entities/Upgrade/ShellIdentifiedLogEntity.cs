using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogShellIdentifiedMessage))]
    [CollectionName(CollectionNames.UPGRADE_SHELL_IDENTIFIED, DisplayCollectionNames.UPGRADE_SHELL_IDENTIFIED)]
    internal class ShellIdentifiedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO Shell { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}