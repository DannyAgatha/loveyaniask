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
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyChangeAuthorityEventHandler : IAsyncEventProcessor<FamilyChangeAuthorityEvent>
    {
        private readonly ICharacterService _characterService;
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyChangeAuthorityEventHandler(IGameLanguageService gameLanguage, FamilyConfiguration familyConfiguration, IFamilyService familyService, ISessionManager sessionManager,
            ICharacterService characterService)
        {
            _gameLanguage = gameLanguage;
            _familyConfiguration = familyConfiguration;
            _familyService = familyService;
            _sessionManager = sessionManager;
            _characterService = characterService;
        }

        public async Task HandleAsync(FamilyChangeAuthorityEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IFamily sessionFamily = session.PlayerEntity.Family;
            long memberId = e.MemberId;

            if (sessionFamily == null)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            FamilyAuthority senderAuthority = session.PlayerEntity.GetFamilyAuthority();

            if (senderAuthority != FamilyAuthority.Head && senderAuthority != FamilyAuthority.Deputy)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            if (senderAuthority > e.FamilyAuthority)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            string memberName = e.CharacterName;

            if (!string.IsNullOrEmpty(memberName))
            {
                IClientSession target = _sessionManager.GetSessionByCharacterName(memberName);
                if (target == null)
                {
                    DbServerGetCharacterResponse characterResponse = null;
                    try
                    {
                        characterResponse = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
                        {
                            CharacterName = memberName
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error("[FAMILY_CHANGE_AUTHORITY] Unexpected error: ", ex);
                    }

                    if (characterResponse?.RpcResponseType != RpcResponseType.SUCCESS)
                    {
                        return;
                    }

                    CharacterDTO targetCharacter = characterResponse.CharacterDto;
                    memberId = targetCharacter.Id;
                }
                else
                {
                    memberId = target.PlayerEntity.Id;
                }
            }


            if (senderAuthority == FamilyAuthority.Head && e.FamilyAuthority == senderAuthority)
            {
                if (e.Confirmed != 1)
                {
                    e.Sender.SendQnaPacket($"fmg {((int)e.FamilyAuthority).ToString()} {memberId.ToString()} 1",
                        _gameLanguage.GetLanguage(GameDialogKey.FAMILY_DIALOG_CHANGE_HEAD, session.UserLanguage));
                    return;
                }
            }

            bool validAction = false;
            int numberOfDeputies = 0;
            int numberOfAssistants = 0;

            foreach (FamilyMembership member in sessionFamily.Members.ToArray())
            {
                switch (member.Authority)
                {
                    case FamilyAuthority.Deputy:
                        numberOfDeputies++;
                        break;
                    case FamilyAuthority.Keeper:
                        numberOfAssistants++;
                        break;
                }

                if (member.CharacterId != memberId)
                {
                    continue;
                }

                if (memberId == session.PlayerEntity.Id)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_DIALOG_CANNOT_CHANGE_YOUR_OWN_AUTHORITY, session.UserLanguage));
                    break;
                }

                if (member.Authority == e.FamilyAuthority)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_DIALOG_ALREADY_HAS_THAT_AUTHORITY, session.UserLanguage));
                    break;
                }

                if (senderAuthority > member.Authority)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                    break;
                }

                if ((member.Authority == senderAuthority || senderAuthority == e.FamilyAuthority && senderAuthority != FamilyAuthority.Head))
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_CANT_GIVE_SAME_AUTHORITY, session.UserLanguage));
                    break;
                }

                if (senderAuthority != FamilyAuthority.Head && e.FamilyAuthority < senderAuthority)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                    break;
                }

                if (senderAuthority == FamilyAuthority.Head && e.FamilyAuthority == senderAuthority)
                {
                    if (member.Authority != FamilyAuthority.Deputy)
                    {
                        session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_TARGET_NEEDS_TO_BE_DEPUTY, session.UserLanguage));
                        break;
                    }
                }

                validAction = true;
                memberName = member.Character?.Name;
            }

            if (!validAction)
            {
                return;
            }

            switch (e.FamilyAuthority)
            {
                case FamilyAuthority.Deputy when _familyConfiguration.DeputyLimit <= numberOfDeputies:
                    e.Sender.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_DEPUTIES_LIMIT_REACHED, e.Sender.UserLanguage));
                    return;
                case FamilyAuthority.Keeper when _familyConfiguration.KeeperLimit <= numberOfAssistants:
                    e.Sender.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_KEEPER_LIMIT_REACHED, e.Sender.UserLanguage));
                    return;
                default:
                    DbServerGetCharacterResponse characterResponse = null;
                    try
                    {
                        characterResponse = await _characterService.GetCharacterById(new DbServerGetCharacterByIdRequest
                        {
                            CharacterId = e.MemberId
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error("[FAMILY_CHANGE_AUTHORITY] Unexpected error: ", ex);
                    }

                    if (characterResponse?.RpcResponseType == RpcResponseType.SUCCESS)
                    {
                        memberName ??= characterResponse.CharacterDto.Name;
                    }

                    var list = new List<FamilyChangeContainer>();
                    if (e.FamilyAuthority == FamilyAuthority.Head)
                    {
                        list.Add(new FamilyChangeContainer
                        {
                            CharacterId = session.PlayerEntity.Id,
                            RequestedFamilyAuthority = FamilyAuthority.Deputy
                        });

                        await session.FamilyAddLogAsync(FamilyLogType.AuthorityChanged, session.PlayerEntity.Name, ((byte)FamilyAuthority.Deputy).ToString(), session.PlayerEntity.Name);
                    }


                    list.Add(new FamilyChangeContainer
                    {
                        CharacterId = memberId,
                        RequestedFamilyAuthority = e.FamilyAuthority
                    });

                    await session.FamilyAddLogAsync(FamilyLogType.AuthorityChanged, session.PlayerEntity.Name, ((byte)e.FamilyAuthority).ToString(), memberName);

                    await _familyService.ChangeAuthorityByIdAsync(new FamilyChangeAuthorityRequest
                    {
                        FamilyMembers = list
                    });
                    break;
            }
        }
    }
}