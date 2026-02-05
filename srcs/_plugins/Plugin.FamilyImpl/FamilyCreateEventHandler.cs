// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Prestige;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;
using WingsEmu.Packets.Enums.Language;

namespace Plugin.FamilyImpl
{
    public class FamilyCreateEventHandler : IAsyncEventProcessor<FamilyCreateEvent>
    {
        private readonly IForbiddenNamesManager _bannedNamesConfiguration;
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyManager _familyManager;
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ISessionManager _sessionManager;
        private readonly IAsyncEventPipeline _eventPipeline;    

        public FamilyCreateEventHandler(IFamilyService familyService, IGameLanguageService gameLanguage, FamilyConfiguration familyConfiguration,
            ISessionManager sessionManager, IFamilyManager familyManager, IRandomGenerator randomGenerator, IForbiddenNamesManager bannedNamesConfiguration, IAsyncEventPipeline eventPipeline)
        {
            _familyService = familyService;
            _gameLanguage = gameLanguage;
            _familyConfiguration = familyConfiguration;
            _sessionManager = sessionManager;
            _familyManager = familyManager;
            _randomGenerator = randomGenerator;
            _bannedNamesConfiguration = bannedNamesConfiguration;
            _eventPipeline = eventPipeline;
        }

        public async Task HandleAsync(FamilyCreateEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                return;
            }

            if (session.CantPerformActionOnAct4())
            {
                return;
            }

            if (session.PlayerEntity.IsInRaidParty)
            {
                return;
            }

            if (session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_ALREADY_IN_FAMILY, session.UserLanguage));
                return;
            }

            if (!session.HasEnoughGold(_familyConfiguration.CreationPrice))
            {
                session.SendInfoi(Game18NConstString.NotEnoughGold5);
                return;
            }

            int nameLength = e.Name.Length;
            if (nameLength < _familyConfiguration.MinimumNameLength || nameLength > _familyConfiguration.MaximumNameLength)
            {
                return;
            }

            PlayerGroup playerGroup = session.PlayerEntity.GetGroup();

            if (session.PlayerEntity.IsInGroup())
            {
                if (playerGroup.Members.Any(x => x.IsInFamily()))
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_CREATION_SOMEONE_ALREADY_IN_FAMILY, session.UserLanguage));
                    return;
                }

                if (playerGroup.Members.Count < _familyConfiguration.CreationGroupMembersRequired)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_GROUP_NOT_FULL, session.UserLanguage));
                    return;
                }
            }
            else if (_familyConfiguration.CreationIsGroupRequired)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_CREATION_GROUP_REQUIRED, session.UserLanguage));
                return;
            }

            var regex = new Regex(@"^[a-zA-Z0-9_\-\*]*$");
            if (regex.Matches(e.Name).Count != 1)
            {
                return;
            }

            string lowerName = e.Name.ToLowerInvariant();

            if (_bannedNamesConfiguration.IsBanned(lowerName, out string bannedName))
            {
                session.SendInfo(_gameLanguage.GetLanguageFormat(GameDialogKey.FAMILY_INFO_CREATION_BANNED_NAME, session.UserLanguage, bannedName));
                return;
            }

            var membersList = new List<FamilyMembershipDto>();
            DateTime now = DateTime.UtcNow;

            if (session.PlayerEntity.IsInGroup())
            {
                foreach (IPlayerEntity member in playerGroup.Members)
                {
                    if (member.Id == session.PlayerEntity.Id)
                    {
                        continue;
                    }

                    membersList.Add(new FamilyMembershipDto
                    {
                        CharacterId = member.Id,
                        Authority = FamilyAuthority.Deputy,
                        DailyMessage = null,
                        Experience = 0,
                        Title = FamilyTitle.Nothing,
                        JoinDate = now
                    });
                }
            }

            membersList.Add(new FamilyMembershipDto
            {
                CharacterId = session.PlayerEntity.Id,
                Authority = FamilyAuthority.Head,
                DailyMessage = null,
                Experience = 0,
                Title = FamilyTitle.Nothing,
                JoinDate = now
            });

            FamilyCreateResponse response = await _familyService.CreateFamilyAsync(new FamilyCreateRequest
            {
                Name = e.Name,
                Level = 1,
                MembershipCapacity = _familyConfiguration.DefaultMembershipCapacity,
                Faction = (byte)_randomGenerator.RandomNumber(1, 3),
                Members = membersList
            });

            if (response.Status == FamilyCreateResponseType.NAME_ALREADY_TAKEN)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NAME_ALREADY_USED, session.UserLanguage));
                return;
            }

            FamilyDTO familyDto = response.Family;
            _familyManager.AddFamily(response.Family, membersList);
            IFamily family = null;

            foreach (FamilyMembershipDto member in membersList)
            {
                member.FamilyId = familyDto.Id;
                _familyManager.AddOrReplaceMember(member);

                IClientSession localSession = _sessionManager.GetSessionByCharacterId(member.CharacterId);

                if (localSession == null)
                {
                    continue;
                }

                family ??= localSession.PlayerEntity.Family;

                if (localSession.PlayerEntity.Faction != (FactionType)family.Faction)
                {
                    await localSession.EmitEventAsync(new ChangeFactionEvent
                    {
                        NewFaction = (FactionType)familyDto.Faction
                    });
                }

                localSession.BroadcastGidx(family, _gameLanguage);
            }

            if (family == null)
            {
                return;
            }

            FamilyPacketExtensions.SendFamilyMembersInfoToMembers(family, _sessionManager, _familyConfiguration);
            family.SendFmiPacket(_sessionManager);

            session.PlayerEntity.RemoveGold(_familyConfiguration.CreationPrice);
            session.EmitEvent(new FamilyCreatedEvent());
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, _familyConfiguration.CreationPrice));
            await _eventPipeline.ProcessEventAsync(new PrestigeProgressEvent(session, PrestigeTaskType.SPEND_GOLD_NPC, amount: _familyConfiguration.CreationPrice),cancellation);
        }
    }
}