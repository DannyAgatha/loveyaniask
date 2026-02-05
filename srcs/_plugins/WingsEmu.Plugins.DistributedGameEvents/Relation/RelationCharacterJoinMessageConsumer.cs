using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.DTOs.Relations;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    public class RelationCharacterJoinMessageConsumer : IMessageConsumer<RelationCharacterJoinMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public RelationCharacterJoinMessageConsumer(ISessionManager sessionManager, IGameLanguageService gameLanguage)
        {
            _sessionManager = sessionManager;
            _gameLanguage = gameLanguage;
        }

        public async Task HandleAsync(RelationCharacterJoinMessage notification, CancellationToken token)
        {
            long characterId = notification.CharacterId;
            string characterName = notification.CharacterName;
            IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);

            List<CharacterRelationDTO> relations = notification.Relations;
            foreach (CharacterRelationDTO relation in relations)
            {
                session?.PlayerEntity.AddRelation(relation);

                if (relation.RelationType == CharacterRelationType.Blocked)
                {
                    continue;
                }

                IClientSession target = _sessionManager.GetSessionByCharacterId(relation.RelatedCharacterId);
                if (target == null || !_sessionManager.IsOnline(relation.RelatedCharacterId))
                {
                    continue;
                }

                string message = _gameLanguage.GetLanguageFormat(GameDialogKey.FRIEND_CHATMESSAGE_CHARACTER_LOGGED_IN, target.UserLanguage, characterName);
                target.SendInformationChatMessage(message);
                target.SendFriendOnlineInfo(characterId, characterName, true);
            }

            session?.RefreshFriendList(_sessionManager);
            session?.RefreshBlackList();
        }
    }
}