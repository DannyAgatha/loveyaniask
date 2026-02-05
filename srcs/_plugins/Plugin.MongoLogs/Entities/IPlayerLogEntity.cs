using System;

namespace Plugin.MongoLogs.Entities
{
    public interface IPlayerLogEntity
    {
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}