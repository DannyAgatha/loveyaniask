using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.DTOs.Relations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    public class RelationCharacterAddMessageConsumer : IMessageConsumer<RelationCharacterAddMessage>
    {
        private readonly ISessionManager _sessionManager;

        public RelationCharacterAddMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(RelationCharacterAddMessage notification, CancellationToken token)
        {
            CharacterRelationDTO senderRelation = notification.SenderRelation;
            CharacterRelationDTO targetRelation = notification.TargetRelation;

            IClientSession session = null;
            IClientSession target = null;

            if (senderRelation != null)
            {
                session = _sessionManager.GetSessionByCharacterId(senderRelation.CharacterId);
            }

            if (targetRelation != null)
            {
                target = _sessionManager.GetSessionByCharacterId(targetRelation.CharacterId);
            }

            if (senderRelation?.RelationType != CharacterRelationType.Blocked)
            {
                target?.PlayerEntity.AddRelation(targetRelation);
            }

            session?.PlayerEntity.AddRelation(senderRelation);

            switch (senderRelation?.RelationType)
            {
                case CharacterRelationType.Blocked:
                    session?.RefreshBlackList();
                    break;
                case CharacterRelationType.Spouse:
                case CharacterRelationType.Friend:
                    session?.RefreshFriendList(_sessionManager);
                    target?.RefreshFriendList(_sessionManager);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}