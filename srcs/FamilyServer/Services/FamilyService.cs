// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyServer.Logs;
using FamilyServer.Managers;
using Foundatio.AsyncEx;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.ServiceBus;
using Plugin.Database.Families;
using Plugin.FamilyImpl.Achievements;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Communication;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game.Families;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Families;

namespace FamilyServer.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IExpirableLockService _expirableLockService;
        private readonly FamilyLogManager _familyLogManager;
        private readonly FamilyManager _familyManager;
        private readonly FamilyMembershipManager _familyMembershipManager;
        private readonly IMessagePublisher<FamilyChangeFactionMessage> _messagePublisherChangedFaction;
        private readonly IMessagePublisher<FamilyCreatedMessage> _messagePublisherCreated;
        private readonly IMessagePublisher<FamilyDisbandMessage> _messagePublisherDisbandedFamily;
        private readonly IMessagePublisher<FamilyUpdateMessage> _messagePublisherFamilyUpdate;
        private readonly IMessagePublisher<FamilyMemberAddedMessage> _messagePublisherMemberAdded;
        private readonly IMessagePublisher<FamilyMemberRemovedMessage> _messagePublisherMemberRemoved;
        private readonly IMessagePublisher<FamilyMemberUpdateMessage> _messagePublisherMemberUpdate;
        private readonly IMessagePublisher<FamilyCreateBuffMessage> _messagePublisherCreateBuff;
        private readonly FamilyMissionsConfiguration _familyMissionsConfiguration;
        private readonly AsyncReaderWriterLock _readerWriter = new();

        public FamilyService(FamilyManager familyManager, IMessagePublisher<FamilyMemberAddedMessage> messagePublisherMemberAdded, IMessagePublisher<FamilyCreatedMessage> messagePublisherCreated,
            IMessagePublisher<FamilyMemberUpdateMessage> messagePublisherMemberUpdate, IMessagePublisher<FamilyMemberRemovedMessage> messagePublisherMemberRemoved,
            FamilyLogManager familyLogManager, FamilyMembershipManager familyMembershipManager, IMessagePublisher<FamilyUpdateMessage> messagePublisherFamilyUpdate,
            IMessagePublisher<FamilyChangeFactionMessage> messagePublisherChangedFaction, IMessagePublisher<FamilyDisbandMessage> messagePublisherDisbandedFamily,
            IExpirableLockService expirableLockService, IMessagePublisher<FamilyCreateBuffMessage> messagePublisherCreateBuff, FamilyMissionsConfiguration familyMissionsConfiguration)
        {
            _familyManager = familyManager;
            _messagePublisherMemberAdded = messagePublisherMemberAdded;
            _messagePublisherCreated = messagePublisherCreated;
            _messagePublisherMemberUpdate = messagePublisherMemberUpdate;
            _messagePublisherMemberRemoved = messagePublisherMemberRemoved;
            _familyLogManager = familyLogManager;
            _familyMembershipManager = familyMembershipManager;
            _messagePublisherFamilyUpdate = messagePublisherFamilyUpdate;
            _messagePublisherChangedFaction = messagePublisherChangedFaction;
            _messagePublisherDisbandedFamily = messagePublisherDisbandedFamily;
            _expirableLockService = expirableLockService;
            _messagePublisherCreateBuff = messagePublisherCreateBuff;
            _familyMissionsConfiguration = familyMissionsConfiguration;
        }

        public async ValueTask<FamilyCreateResponse> CreateFamilyAsync(FamilyCreateRequest req)
        {
            using IDisposable writerLock = await _readerWriter.WriterLockAsync();
            {
                if (await _familyManager.GetFamilyByNameAsync(req.Name) != null)
                {
                    return new FamilyCreateResponse
                    {
                        Status = FamilyCreateResponseType.NAME_ALREADY_TAKEN
                    };
                }

                FamilyDTO familyDto = await _familyManager.AddFamilyAsync(new FamilyDTO
                {
                    Name = req.Name,
                    Level = req.Level,
                    Faction = req.Faction,
                    Upgrades = new FamilyUpgradeDto(),
                    Missions = CreateBasicFamilyMissions(),
                    Achievements = CreateBasicAchievements(),
                    CurrentDayRankStat = new FamilyRankingDto(),
                    CurrentMonthRankStat = new FamilyRankingDto(),
                    GlobalRankStat = new FamilyRankingDto(),
                    PreviousMonthRankStat = new FamilyRankingDto(),
                });

                foreach (FamilyMembershipDto member in req.Members)
                {
                    member.FamilyId = familyDto.Id;
                }

                await _familyMembershipManager.AddFamilyMembershipsAsync(req.Members);

                await _messagePublisherCreated.PublishAsync(new FamilyCreatedMessage
                {
                    FamilyName = req.Name
                });

                return new FamilyCreateResponse
                {
                    Status = FamilyCreateResponseType.SUCCESS,
                    Family = familyDto
                };
            }
        }

        public async ValueTask<BasicRpcResponse> DisbandFamilyAsync(FamilyDisbandRequest request)
        {
            IReadOnlyCollection<FamilyMembershipDto> memberships = await _familyMembershipManager.GetFamilyMembershipsByFamilyIdAsync(request.FamilyId);
            if (memberships == null)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            foreach (FamilyMembershipDto membership in memberships)
            {
                await _familyMembershipManager.RemoveFamilyMembershipByCharAndFamIdAsync(membership);
            }

            await _familyManager.RemoveFamilyByIdAsync(request.FamilyId);
            await _messagePublisherDisbandedFamily.PublishAsync(new FamilyDisbandMessage
            {
                FamilyId = request.FamilyId
            });

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<EmptyResponse> ChangeAuthorityByIdAsync(FamilyChangeAuthorityRequest request)
        {
            if (request.FamilyMembers.Count <= 0)
            {
                return new EmptyResponse();
            }

            var list = new List<FamilyMembershipDto>();

            foreach (FamilyChangeContainer container in request.FamilyMembers)
            {
                FamilyMembershipDto membership = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(container.CharacterId);
                if (membership == null)
                {
                    continue;
                }

                membership.Authority = container.RequestedFamilyAuthority;
                list.Add(membership);
            }

            await _familyMembershipManager.SaveFamilyMembershipsAsync(list);

            await _messagePublisherMemberUpdate.PublishAsync(new FamilyMemberUpdateMessage
            {
                UpdatedMembers = list,
                ChangedInfoMemberUpdate = ChangedInfoMemberUpdate.Authority
            });
            return new EmptyResponse();
        }

        public async ValueTask<FamilyChangeFactionResponse> ChangeFactionByIdAsync(FamilyChangeFactionRequest request)
        {
            FactionType newFaction = request.NewFaction;
            FamilyDTO familyDto = await _familyManager.GetFamilyByFamilyIdAsync(request.FamilyId);
            if (familyDto == null)
            {
                return new FamilyChangeFactionResponse
                {
                    Status = FamilyChangeFactionResponseType.GENERIC_ERROR
                };
            }

            if (familyDto.Faction == (byte)newFaction)
            {
                return new FamilyChangeFactionResponse
                {
                    Status = FamilyChangeFactionResponseType.ALREADY_THAT_FACTION
                };
            }

            if (!await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:family:{familyDto.Id}:change-faction", DateTime.UtcNow.AddMinutes(1)))
            {
                return new FamilyChangeFactionResponse
                {
                    Status = FamilyChangeFactionResponseType.UNDER_COOLDOWN
                };
            }

            familyDto.Faction = (byte)newFaction;
            await _familyManager.AddFamilyAsync(familyDto);

            await _messagePublisherChangedFaction.PublishAsync(new FamilyChangeFactionMessage
            {
                FamilyId = request.FamilyId,
                NewFaction = newFaction
            });

            return new FamilyChangeFactionResponse
            {
                Status = FamilyChangeFactionResponseType.SUCCESS
            };
        }

        public async ValueTask<EmptyResponse> ChangeTitleByIdAsync(FamilyChangeTitleRequest request)
        {
            FamilyMembershipDto membership = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(request.CharacterId);
            if (membership == null)
            {
                return new EmptyResponse();
            }

            membership.Title = request.RequestedFamilyTitle;

            await _familyMembershipManager.SaveFamilyMembershipAsync(membership);

            await _messagePublisherMemberUpdate.PublishAsync(new FamilyMemberUpdateMessage
            {
                UpdatedMembers = new List<FamilyMembershipDto> { membership },
                ChangedInfoMemberUpdate = ChangedInfoMemberUpdate.Authority
            });
            return new EmptyResponse();
        }

        public async ValueTask<FamilyUpgradeResponse> TryAddFamilyUpgrade(FamilyUpgradeRequest request)
        {
            FamilyDTO family = await _familyManager.GetFamilyByFamilyIdAsync(request.FamilyId);

            if (family == null)
            {
                return new FamilyUpgradeResponse { ResponseType = FamilyUpgradeAddResponseType.GENERIC_SERVER_ERROR };
            }

            FamilyUpgradeType familyUpgrade = request.FamilyUpgradeType;
            short value = request.Value;

            family.Upgrades ??= new FamilyUpgradeDto();

            family.Upgrades.UpgradesBought ??= new HashSet<int>();

            if (family.Upgrades.UpgradesBought.Contains(request.UpgradeId))
            {
                return new FamilyUpgradeResponse { ResponseType = FamilyUpgradeAddResponseType.UPGRADE_ALREADY_UNLOCKED };
            }

            family.Upgrades.UpgradesBought.Add(request.UpgradeId);

            family.Upgrades.UpgradeValues ??= new Dictionary<FamilyUpgradeType, (short, short?)>();
            family.Upgrades.UpgradeValues[request.FamilyUpgradeType] = (request.Value, request.SecondValue);

            FamilyDTO familyDto = await _familyManager.SaveFamilyAsync(family);

            await _messagePublisherFamilyUpdate.PublishAsync(new FamilyUpdateMessage
            {
                ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.Upgrades,
                Families = new[] { familyDto }
            });
            return new FamilyUpgradeResponse { ResponseType = FamilyUpgradeAddResponseType.SUCCESS };
        }

        public async ValueTask<EmptyResponse> AddMemberToFamilyAsync(FamilyAddMemberRequest request)
        {
            await _familyMembershipManager.AddFamilyMembershipAsync(request.Member);
            await _messagePublisherMemberAdded.PublishAsync(new FamilyMemberAddedMessage
            {
                AddedMember = request.Member,
                Nickname = request.Nickname,
                SenderId = request.SenderId
            });
            return new EmptyResponse();
        }

        public async ValueTask<EmptyResponse> MemberDisconnectedAsync(FamilyMemberDisconnectedRequest request)
        {
            FamilyMembershipDto member = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(request.CharacterId);
            if (member == null)
            {
                return new EmptyResponse();
            }

            member.LastOnlineDate = request.DisconnectionTime;
            await _familyMembershipManager.SaveFamilyMembershipAsync(member);
            return new EmptyResponse();
        }

        public async ValueTask<EmptyResponse> RemoveMemberToFamilyAsync(FamilyRemoveMemberRequest request)
        {
            FamilyMembershipDto membership = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(request.CharacterId);
            if (membership == null || membership.FamilyId != request.FamilyId)
            {
                return new EmptyResponse();
            }

            await _familyMembershipManager.RemoveFamilyMembershipByCharAndFamIdAsync(membership);
            await _messagePublisherMemberRemoved.PublishAsync(new FamilyMemberRemovedMessage
            {
                CharacterId = request.CharacterId,
                FamilyId = request.FamilyId
            });
            return new EmptyResponse();
        }

        public async ValueTask<BasicRpcResponse> RemoveMemberByCharIdAsync(FamilyRemoveMemberByCharIdRequest request)
        {
            FamilyMembershipDto membership = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(request.CharacterId);
            if (membership == null)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            await _familyMembershipManager.RemoveFamilyMembershipByCharAndFamIdAsync(membership);
            await _messagePublisherMemberRemoved.PublishAsync(new FamilyMemberRemovedMessage
            {
                CharacterId = request.CharacterId,
                FamilyId = membership.FamilyId
            });

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<FamilyIdResponse> GetFamilyByIdAsync(FamilyIdRequest req)
        {
            FamilyDTO family = await _familyManager.GetFamilyByFamilyIdAsync(req.FamilyId);
            List<FamilyMembershipDto> members = await _familyMembershipManager.GetFamilyMembershipsByFamilyIdAsync(req.FamilyId);
            List<FamilyLogDto> logs = await _familyLogManager.GetFamilyLogsByFamilyId(req.FamilyId);

            await RemoveLevelAchievements(family);
            AddMissingFamilyMissions(family);
            DateTime resetDate = DateTime.UtcNow.Date;
            if (family.Missions?.Missions != null)
            {
                foreach (FamilyMissionDto mission in family.Missions.Missions.Values)
                {
                    // progressType => 1 = Completed
                    // progressType => 2 = InProgress
                    bool hasCompleted = mission.CompletionDate.HasValue;

                    if (!hasCompleted)
                    {
                        continue;
                    }

                    if (mission.CompletionDate.Value.Date >= resetDate)
                    {
                        continue;
                    }

                    mission.CompletionDate = null;
                    mission.Count = 0;
                }
            }

            return new FamilyIdResponse
            {
                Family = family,
                Members = members,
                Logs = logs
            };
        }

        public async ValueTask<FamilyListMembersResponse> GetFamilyMembersByFamilyId(FamilyIdRequest req)
        {
            List<FamilyMembershipDto> members = await _familyMembershipManager.GetFamilyMembershipsByFamilyIdAsync(req.FamilyId);
            return new FamilyListMembersResponse
            {
                Members = members
            };
        }

        public async ValueTask<MembershipResponse> GetMembershipByCharacterIdAsync(MembershipRequest req)
        {
            FamilyMembershipDto membership = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(req.CharacterId);
            return new MembershipResponse
            {
                Membership = membership
            };
        }

        public async ValueTask<MembershipTodayResponse> CanPerformTodayMessageAsync(MembershipTodayRequest req)
        {
            long characterId = req.CharacterId;
            string characterName = req.CharacterName;
            FamilyMembershipDto familyMember = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(characterId);
            if (familyMember == null)
            {
                return new MembershipTodayResponse
                {
                    CanPerformAction = false
                };
            }

            if (!await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:family:{familyMember.FamilyId}:{characterId}:quote-of-the-day", DateTime.UtcNow.Date.AddDays(1)))
            {
                return new MembershipTodayResponse
                {
                    CanPerformAction = false
                };
            }

            return new MembershipTodayResponse
            {
                CanPerformAction = true
            };
        }

        public async ValueTask<BasicRpcResponse> UpdateFamilySettingsAsync(FamilySettingsRequest request)
        {
            FamilyDTO familyDto = await _familyManager.GetFamilyByFamilyIdAsync(request.FamilyId);
            if (familyDto == null)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            byte value = request.Value;

            switch (request.Authority)
            {
                case FamilyAuthority.Keeper:
                    switch (request.FamilyActionType)
                    {
                        case FamilyActionType.SendInvite:
                            familyDto.AssistantCanInvite = value == 1;
                            break;
                        case FamilyActionType.Notice:
                            familyDto.AssistantCanNotice = value == 1;
                            break;
                        case FamilyActionType.FamilyShout:
                            familyDto.AssistantCanShout = value == 1;
                            break;
                        case FamilyActionType.FamilyWarehouseHistory:
                            familyDto.AssistantCanGetHistory = value == 1;
                            break;
                        case FamilyActionType.FamilyWarehouse:
                            if (!Enum.TryParse(value.ToString(), out FamilyWarehouseAuthorityType authorityType))
                            {
                                return new BasicRpcResponse
                                {
                                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                                };
                            }

                            familyDto.AssistantWarehouseAuthorityType = authorityType;
                            break;
                        default:
                            return new BasicRpcResponse
                            {
                                ResponseType = RpcResponseType.UNKNOWN_ERROR
                            };
                    }

                    break;

                case FamilyAuthority.Member:
                    switch (request.FamilyActionType)
                    {
                        case FamilyActionType.SendInvite: // Member History
                            familyDto.MemberCanGetHistory = value == 1;
                            break;
                        case FamilyActionType.Notice: // Member Warehouse Authority
                            if (!Enum.TryParse(value.ToString(), out FamilyWarehouseAuthorityType authorityType))
                            {
                                return new BasicRpcResponse
                                {
                                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                                };
                            }

                            familyDto.MemberWarehouseAuthorityType = authorityType;
                            break;
                        default:
                            return new BasicRpcResponse
                            {
                                ResponseType = RpcResponseType.UNKNOWN_ERROR
                            };
                    }

                    break;
                default:
                    return new BasicRpcResponse
                    {
                        ResponseType = RpcResponseType.UNKNOWN_ERROR
                    };
            }

            FamilyDTO family = await _familyManager.SaveFamilyAsync(familyDto);

            await _messagePublisherFamilyUpdate.PublishAsync(new FamilyUpdateMessage
            {
                ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.Settings,
                Families = new[] { family }
            });

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }
        
        public async ValueTask<FamilyReputationRankingResponse> GetFamilyReputation(FamilyReputationRankingRequest req)
        {
            return new FamilyReputationRankingResponse
            {
                Reputation = await _familyManager.GetReputationInTotalByIdAsync(req.FamilyId)
            };
        }

        public async ValueTask<FamilyAllResponse> GetFamilies()
        {
            return new FamilyAllResponse
            {
                Families = await _familyManager.GetFamiliesAsync()
            };
        }

        public async ValueTask<EmptyResponse> SaveFamilyAsync(FamilySaveRequest req)
        {
            await _familyManager.SaveFamilyAsync(req.FamilyDto);
            return new EmptyResponse();
        }

        public async ValueTask<EmptyResponse> CreateFamilyBuffAsync(FamilyBuffCreateRequest req)
        {
            await _messagePublisherCreateBuff.PublishAsync(new FamilyCreateBuffMessage
            {
                FamilyId = req.FamilyBuffsCrossChannel.FamilyId,
                FamilyName = req.FamilyBuffsCrossChannel.FamilyName,
                BuffVnum = req.FamilyBuffsCrossChannel.BuffVnum,
                ItemVnum = req.FamilyBuffsCrossChannel.ItemVnum,
                EndTime = req.FamilyBuffsCrossChannel.EndTime,
                FactionType = req.FamilyBuffsCrossChannel.FactionType,
            });

            return new EmptyResponse();
        }

        public async ValueTask<EmptyResponse> ResetFamilyMissions()
        {
            DateTime resetDate = DateTime.UtcNow.Date;
            List<FamilyDTO> families = _familyManager.GetFamiliesInMemory();
            HashSet<FamilyDTO> toUpdate = [];

            foreach (FamilyDTO family in families)
            {
                if (family?.Missions?.Missions == null)
                {
                    continue;
                }

                foreach (FamilyMissionDto mission in family.Missions.Missions.Values)
                {
                    // progressType => 1 = Completed
                    // progressType => 2 = InProgress
                    bool hasCompleted = mission.CompletionDate.HasValue;

                    if (!hasCompleted)
                    {
                        continue;
                    }

                    if (mission.CompletionDate.Value.Date >= resetDate)
                    {
                        continue;
                    }

                    mission.CompletionDate = null;
                    mission.Count = 0;

                    toUpdate.Add(family);
                }
            }

            await _messagePublisherFamilyUpdate.PublishAsync(new FamilyUpdateMessage
            {
                Families = toUpdate,
                ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.AchievementsAndMissions
            });

            return new EmptyResponse();
        }

        private FamilyAchievementsDto CreateBasicAchievements()
        {
            FamilyAchievementsDto newAchievements = new()
            {
                Achievements = new Dictionary<int, FamilyAchievementCompletionDto>(),
                Progress = new Dictionary<int, FamilyAchievementProgressDto>()
            };

            Dictionary<int, FamilyAchievementProgressDto> progress = newAchievements.Progress;
            progress[(short)FamilyAchievementsVnum.FAMILY_LEVEL_2_UNLOCKED] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.FAMILY_LEVEL_2_UNLOCKED };
            progress[(short)FamilyAchievementsVnum.ENTER_20_QUOTES_OF_THE_DAY] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.ENTER_20_QUOTES_OF_THE_DAY };
            progress[(short)FamilyAchievementsVnum.DEFEAT_ANY_ACT4_DUNGEON_10_TIMES] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.DEFEAT_ANY_ACT4_DUNGEON_10_TIMES };
            progress[(short)FamilyAchievementsVnum.DEFEAT_MORCOS_ACT4_DUNGEON_1_TIME] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.DEFEAT_MORCOS_ACT4_DUNGEON_1_TIME };
            progress[(short)FamilyAchievementsVnum.DEFEAT_HATUS_ACT4_DUNGEON_1_TIME] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.DEFEAT_HATUS_ACT4_DUNGEON_1_TIME };
            progress[(short)FamilyAchievementsVnum.DEFEAT_CALVINAS_ACT4_DUNGEON_1_TIME] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.DEFEAT_CALVINAS_ACT4_DUNGEON_1_TIME };
            progress[(short)FamilyAchievementsVnum.DEFEAT_BERIOS_ACT4_DUNGEON_1_TIME] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.DEFEAT_BERIOS_ACT4_DUNGEON_1_TIME };
            progress[(short)FamilyAchievementsVnum.COMPLETE_10_RAINBOW_BATTLE] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.COMPLETE_10_RAINBOW_BATTLE };
            progress[(short)FamilyAchievementsVnum.WIN_5_RAINBOW_BATTLE] = new FamilyAchievementProgressDto { Id = (short)FamilyAchievementsVnum.WIN_5_RAINBOW_BATTLE };
            return newAchievements;
        }

        private FamilyMissionsDto CreateBasicFamilyMissions()
        {
            FamilyMissionsDto newFamilyMissions = new()
            {
                Missions = new Dictionary<int, FamilyMissionDto>()
            };

            Dictionary<int, FamilyMissionDto> missions = newFamilyMissions.Missions;
            missions[(int)FamilyMissionVnums.DAILY_DEFEAT_5_CUBY_RAID] = new FamilyMissionDto { Id = (int)FamilyMissionVnums.DAILY_DEFEAT_5_CUBY_RAID };
            missions[(int)FamilyMissionVnums.DAILY_DEFEAT_5_GINSENG_RAID] = new FamilyMissionDto { Id = (int)FamilyMissionVnums.DAILY_DEFEAT_5_GINSENG_RAID };
            missions[(int)FamilyMissionVnums.DAILY_DEFEAT_5_CASTRA_RAID] = new FamilyMissionDto { Id = (int)FamilyMissionVnums.DAILY_DEFEAT_5_CASTRA_RAID };
            missions[(int)FamilyMissionVnums.DAILY_DEFEAT_5_GIANT_SPIDER_RAID] = new FamilyMissionDto { Id = (int)FamilyMissionVnums.DAILY_DEFEAT_5_GIANT_SPIDER_RAID };
            missions[(int)FamilyMissionVnums.DAILY_DEFEAT_10_INSTANT_BATTLES] = new FamilyMissionDto { Id = (int)FamilyMissionVnums.DAILY_DEFEAT_10_INSTANT_BATTLES };
            return newFamilyMissions;
        }
        
        private void AddMissingFamilyMissions(FamilyDTO family)
        {
            if (family.Missions?.Missions is null)
            {
                return;
            }

            foreach (FamilyMissionSpecificConfiguration familyMissionSpecificConfiguration in _familyMissionsConfiguration)
            {
                if (family.Missions.Missions.ContainsKey(familyMissionSpecificConfiguration.MissionId))
                {
                    continue;
                }

                if (family.Level < familyMissionSpecificConfiguration.MinimumRequiredLevel)
                {
                    continue;
                }

                family.Missions.Missions.TryAdd(familyMissionSpecificConfiguration.MissionId, new FamilyMissionDto { Id = familyMissionSpecificConfiguration.MissionId });
            }
        }

        private async Task RemoveLevelAchievements(FamilyDTO family)
        {
            if (family == null)
            {
                return;
            }

            if (family.Level <= 1)
            {
                return;
            }

            if (family.Achievements?.Achievements == null)
            {
                return;
            }

            byte familyLevel = family.Level;
            short secondLevelFamily = (short)FamilyAchievementsVnum.FAMILY_LEVEL_2_UNLOCKED - 1;
            bool isFirst = true;
            bool addNewProgress = true;
            var toRemove = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    secondLevelFamily++;
                }

                if (!family.Achievements.Achievements.ContainsKey(secondLevelFamily))
                {
                    continue;
                }

                if (familyLevel > i)
                {
                    continue;
                }

                if (addNewProgress)
                {
                    addNewProgress = false;
                    family.Achievements.Progress[secondLevelFamily] = new FamilyAchievementProgressDto
                    {
                        Id = secondLevelFamily
                    };
                }

                toRemove.Add(secondLevelFamily);
            }

            foreach (int remove in toRemove)
            {
                family.Achievements.Achievements.Remove(remove);
            }

            await _familyManager.SaveFamilyAsync(family);
        }
    }
}