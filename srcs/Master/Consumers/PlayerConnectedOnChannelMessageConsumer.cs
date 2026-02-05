using System.Threading;
using System.Threading.Tasks;
using Master.Managers;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Player;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace Master.Consumers
{
    public class PlayerConnectedOnChannelMessageConsumer : IMessageConsumer<PlayerConnectedOnChannelMessage>
    {
        private readonly ClusterCharacterManager _clusterCharacterManager;

        public PlayerConnectedOnChannelMessageConsumer(ClusterCharacterManager clusterCharacterManager) => _clusterCharacterManager = clusterCharacterManager;

        public Task HandleAsync(PlayerConnectedOnChannelMessage notification, CancellationToken token)
        {
            _clusterCharacterManager.AddClusterCharacter(new ClusterCharacterInfo
            {
                Id = notification.CharacterId,
                Class = notification.Class,
                SubClass = notification.SubClass,
                Gender = notification.Gender,
                Level = notification.Level,
                Name = notification.CharacterName,
                ChannelId = (byte?)notification.ChannelId,
                HeroLevel = notification.HeroLevel,
                HardwareId = notification.HardwareId
            });

            return Task.CompletedTask;
        }
    }
}