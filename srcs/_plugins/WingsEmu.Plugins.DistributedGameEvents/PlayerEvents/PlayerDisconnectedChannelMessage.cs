using System;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.PlayerEvents
{
    [MessageType("player.disconnected")]
    public class PlayerDisconnectedChannelMessage : IMessage
    {
        public int ChannelId { get; set; }
        public long CharacterId { get; set; }
        public string CharacterName { get; set; }
        public DateTime DisconnectionTime { get; set; }
        public long? FamilyId { get; set; }
    }
}