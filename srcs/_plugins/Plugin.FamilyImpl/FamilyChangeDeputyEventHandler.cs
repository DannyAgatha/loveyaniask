using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Character;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyChangeDeputyEventHandler : IAsyncEventProcessor<FamilyChangeDeputyEvent>
    {
        private readonly ICharacterService _characterService;
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyChangeDeputyEventHandler(IGameLanguageService gameLanguage, ISessionManager sessionManager, IFamilyService familyService, ICharacterService characterService)
        {
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
            _familyService = familyService;
            _characterService = characterService;
        }

        public async Task HandleAsync(FamilyChangeDeputyEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            string targetName = e.TargetName;
            string sourceName = e.SourceName;

            long targetId;
            long sourceId;

            if (string.IsNullOrEmpty(targetName))
            {
                return;
            }

            if (string.IsNullOrEmpty(sourceName))
            {
                return;
            }

            if (sourceName == targetName)
            {
                return;
            }

            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            if (!session.PlayerEntity.IsHeadOfFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            IClientSession source = _sessionManager.GetSessionByCharacterName(sourceName);
            if (source == null)
            {
                DbServerGetCharacterResponse characterResponse = null;
                try
                {
                    characterResponse = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
                    {
                        CharacterName = sourceName
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("[FAMILY_CHANGE_DEPUTY] Unexpected error: ", ex);
                }

                if (characterResponse?.RpcResponseType != RpcResponseType.SUCCESS)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage));
                    return;
                }

                CharacterDTO sourceCharacter = characterResponse.CharacterDto;
                sourceId = sourceCharacter.Id;
            }
            else
            {
                sourceId = source.PlayerEntity.Id;
            }

            IClientSession target = _sessionManager.GetSessionByCharacterName(targetName);
            if (target == null)
            {
                DbServerGetCharacterResponse characterResponse = null;
                try
                {
                    characterResponse = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
                    {
                        CharacterName = targetName
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("[FAMILY_CHANGE_DEPUTY] Unexpected error: ", ex);
                }

                if (characterResponse?.RpcResponseType != RpcResponseType.SUCCESS)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage));
                    return;
                }

                CharacterDTO targetCharacter = characterResponse.CharacterDto;
                targetId = targetCharacter.Id;
            }
            else
            {
                targetId = target.PlayerEntity.Id;
            }

            IFamily family = session.PlayerEntity.Family;
            FamilyMembership targetMembership = null;
            FamilyMembership sourceMembership = null;

            foreach (FamilyMembership member in family.Members)
            {
                if (member.CharacterId == sourceId)
                {
                    sourceMembership = member;
                    continue;
                }

                if (member.CharacterId != targetId)
                {
                    continue;
                }

                targetMembership = member;
            }

            if (sourceMembership == null)
            {
                return;
            }

            if (targetMembership == null)
            {
                return;
            }

            FamilyAuthority sourceAuthority = sourceMembership.Authority;
            FamilyAuthority targetAuthority = targetMembership.Authority;

            if (sourceAuthority != FamilyAuthority.Deputy)
            {
                return;
            }

            if (targetAuthority <= FamilyAuthority.Deputy)
            {
                return;
            }

            var list = new List<FamilyChangeContainer>
            {
                new()
                {
                    CharacterId = targetMembership.CharacterId,
                    RequestedFamilyAuthority = sourceAuthority
                },
                new()
                {
                    CharacterId = sourceMembership.CharacterId,
                    RequestedFamilyAuthority = targetAuthority
                }
            };

            await session.FamilyAddLogAsync(FamilyLogType.AuthorityChanged, session.PlayerEntity.Name, ((byte)targetAuthority).ToString(), sourceName);
            await session.FamilyAddLogAsync(FamilyLogType.AuthorityChanged, session.PlayerEntity.Name, ((byte)sourceAuthority).ToString(), targetName);

            await _familyService.ChangeAuthorityByIdAsync(new FamilyChangeAuthorityRequest
            {
                FamilyMembers = list
            });
        }
    }
}