using System.Threading;
using System.Threading.Tasks;
using Master.Managers;
using PhoenixLib.ServiceBus;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace Master.Consumers
{
    public class PlayerDisconnectedChannelMessageConsumer : IMessageConsumer<PlayerDisconnectedChannelMessage>
    {
        private readonly ClusterCharacterManager _clusterCharacterManager;

        public PlayerDisconnectedChannelMessageConsumer(ClusterCharacterManager clusterCharacterManager) => _clusterCharacterManager = clusterCharacterManager;

        public Task HandleAsync(PlayerDisconnectedChannelMessage notification, CancellationToken token)
        {
            _clusterCharacterManager.RemoveClusterCharacter(notification.CharacterId);
            return Task.CompletedTask;
        }
    }
}