using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    public class RelationSendTalkMessageConsumer : IMessageConsumer<RelationSendTalkMessage>
    {
        private readonly ISessionManager _sessionManager;

        public RelationSendTalkMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(RelationSendTalkMessage notification, CancellationToken token)
        {
            long senderId = notification.SenderId;
            long targetId = notification.TargetId;
            string message = notification.Message;

            IClientSession target = _sessionManager.GetSessionByCharacterId(targetId);
            target?.SendFriendMessage(senderId, message);
        }
    }
}