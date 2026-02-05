using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    public class RelationCharacterRemoveMessageConsumer : IMessageConsumer<RelationCharacterRemoveMessage>
    {
        private readonly ISessionManager _sessionManager;

        public RelationCharacterRemoveMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(RelationCharacterRemoveMessage notification, CancellationToken token)
        {
            long characterId = notification.CharacterId;
            long targetId = notification.TargetId;
            CharacterRelationType relationType = notification.RelationType;
            IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);
            IClientSession target = _sessionManager.GetSessionByCharacterId(targetId);

            session?.PlayerEntity.RemoveRelation(targetId, relationType);
            switch (relationType)
            {
                case CharacterRelationType.Blocked:
                    session?.RefreshBlackList();
                    break;
                case CharacterRelationType.Spouse:
                case CharacterRelationType.Friend:
                    session?.RefreshFriendList(_sessionManager);
                    break;
            }

            if (relationType == CharacterRelationType.Blocked)
            {
                return;
            }

            target?.PlayerEntity.RemoveRelation(characterId, relationType);
            switch (relationType)
            {
                case CharacterRelationType.Spouse:
                case CharacterRelationType.Friend:
                    target?.RefreshFriendList(_sessionManager);
                    break;
            }
        }
    }
}