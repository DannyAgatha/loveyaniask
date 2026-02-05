using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.Relation;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.DTOs.Relations;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    public class RelationCharacterLeaveMessageConsumer : IMessageConsumer<RelationCharacterLeaveMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IRelationService _relationService;
        private readonly ISessionManager _sessionManager;

        public RelationCharacterLeaveMessageConsumer(IGameLanguageService gameLanguage, ISessionManager sessionManager, IRelationService relationService)
        {
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
            _relationService = relationService;
        }

        public async Task HandleAsync(RelationCharacterLeaveMessage notification, CancellationToken token)
        {
            long characterId = notification.CharacterId;
            string characterName = notification.CharacterName;

            RelationGetAllResponse response = null;
            try
            {
                response = await _relationService.GetRelationsByIdAsync(new RelationGetAllRequest
                {
                    CharacterId = characterId
                });
            }
            catch (Exception e)
            {
                Log.Error("[RELATION_CHARACTER_LEAVE] Unexpected error: ", e);
            }

            if (response?.ResponseType != RpcResponseType.SUCCESS)
            {
                return;
            }

            IReadOnlyList<CharacterRelationDTO> relations = response.CharacterRelationDtos ?? new List<CharacterRelationDTO>();

            if (!relations.Any())
            {
                return;
            }

            foreach (CharacterRelationDTO relation in relations)
            {
                if (relation.RelationType == CharacterRelationType.Blocked)
                {
                    continue;
                }

                IClientSession target = _sessionManager.GetSessionByCharacterId(relation.RelatedCharacterId);
                if (target == null || !_sessionManager.IsOnline(relation.RelatedCharacterId))
                {
                    continue;
                }

                string message = _gameLanguage.GetLanguageFormat(GameDialogKey.FRIEND_CHATMESSAGE_CHARACTER_LOGGED_OUT, target.UserLanguage, characterName);
                target.SendInformationChatMessage(message);
                target.SendFriendOnlineInfo(characterId, characterName, false);
            }
        }
    }
}