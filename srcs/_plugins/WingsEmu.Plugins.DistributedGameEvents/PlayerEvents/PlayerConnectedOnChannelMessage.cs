// NosEmu
// 


using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Plugins.DistributedGameEvents.PlayerEvents
{
    [MessageType("player.connected")]
    public class PlayerConnectedOnChannelMessage : IMessage
    {
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public GenderType Gender { get; init; }
        public ClassType Class { get; init; }
        public SubClassType SubClass { get; init; }
        public byte Level { get; init; }
        public byte HeroLevel { get; init; }
        public long? FamilyId { get; init; }
        public string HardwareId { get; init; }
    }
}