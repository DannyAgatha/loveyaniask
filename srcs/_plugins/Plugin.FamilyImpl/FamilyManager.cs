using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Logging;
using Plugin.FamilyImpl.Logs;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyManager : IFamilyManager
    {
        private readonly IExpirableLockService _expirableLockService;
        private readonly IKeyValueCache<IFamily> _familyByName;
        private readonly ILongKeyCachedRepository<Family> _familyCache;
        private readonly IFamilyExperienceManager _familyExperienceManager;
        private readonly IFamilyLogManager _familyLogManager;
        private readonly IFamilyService _familyService;

        private readonly ReaderWriterLockSlim _lock = new();
        private readonly ILongKeyCachedRepository<FamilyMembership> _membershipCache;
        private readonly ISessionManager _sessionManager;
        private readonly List<FamilyBuffCrossChannel> _familyBuffs = new();

        public FamilyManager(ILongKeyCachedRepository<Family> familyCache, IFamilyService familyService, ISessionManager sessionManager, ILongKeyCachedRepository<FamilyMembership> membershipCache,
            IFamilyLogManager familyLogManager, IFamilyExperienceManager familyExperienceManager, IKeyValueCache<IFamily> familyByName, IExpirableLockService expirableLockService)
        {
            _familyCache = familyCache;
            _familyService = familyService;
            _sessionManager = sessionManager;
            _membershipCache = membershipCache;
            _familyLogManager = familyLogManager;
            _familyExperienceManager = familyExperienceManager;
            _familyByName = familyByName;
            _expirableLockService = expirableLockService;
        }

        public void AddFamily(FamilyDTO family, IReadOnlyCollection<FamilyMembershipDto> members)
        {
            IFamily oldFamily = _familyCache.Get(family.Id);
            var newFamily = new Family(family, members, new List<FamilyLogDto>(), _sessionManager);
            _familyCache.Set(family.Id, newFamily);
            _familyByName.Set(family.Name, newFamily);
            if (oldFamily == null)
            {
                return;
            }

            newFamily.Members.AddRange(oldFamily.Members);
            oldFamily.Members.Clear();
        }

        public void RemoveFamily(long familyId)
        {
            IFamily cachedFamily = _familyCache.Get(familyId);
            _familyCache.Remove(familyId);

            foreach (FamilyMembership member in cachedFamily.Members)
            {
                _membershipCache.Remove(member.CharacterId);
                IClientSession session = _sessionManager.GetSessionByCharacterId(member.CharacterId);
                session?.PlayerEntity.SetFamilyMembership(null);
            }
        }

        public IFamily GetFamilyByFamilyName(string familyName) => _familyByName.Get(familyName);

        public void AddOrReplaceMember(FamilyMembershipDto membership, IFamily family)
        {
            if (family == null)
            {
                return;
            }

            var newMember = new FamilyMembership(membership, _sessionManager);
            family.Members.RemoveAll(x => x.CharacterId == membership.CharacterId);
            _membershipCache.Set(newMember.CharacterId, newMember);
            if (newMember.Authority == FamilyAuthority.Head)
            {
                family.Head = newMember;
            }

            family.Members.Add(newMember);
            IClientSession session = _sessionManager.GetSessionByCharacterId(membership.CharacterId);
            session?.PlayerEntity.SetFamilyMembership(newMember);
        }

        public void AddOrReplaceMember(FamilyMembershipDto membership)
        {
            AddOrReplaceMember(membership, GetFamilyByFamilyId(membership.FamilyId));
        }

        public void RemoveMember(long characterId, long familyId)
        {
            IFamily family = GetFamilyByFamilyId(familyId);
            family?.Members.RemoveAll(x => x.CharacterId == characterId);

            _membershipCache.Remove(characterId);
            IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);
            session?.PlayerEntity.SetFamilyMembership(null);
        }

        public void MemberDisconnectionUpdate(long characterId, DateTime disconnectionTime)
        {
            FamilyMembership member = GetFamilyMembershipByCharacterId(characterId);
            if (member == null)
            {
                return;
            }

            member.LastOnlineDate = disconnectionTime;
            AddOrReplaceMember(member);
        }

        public FamilyMembership GetFamilyMembershipByCharacterId(long characterId)
        {
            FamilyMembership membership = _membershipCache.Get(characterId);
            if (membership != null)
            {
                return membership;
            }

            MembershipResponse response = _familyService.GetMembershipByCharacterIdAsync(
                new MembershipRequest { CharacterId = characterId }).ConfigureAwait(false).GetAwaiter().GetResult();

            return response.Membership == null ? null : _membershipCache.GetOrSet(response.Membership.CharacterId, () => new FamilyMembership(response.Membership, _sessionManager));
        }

        public void AddToFamilyLogs(IReadOnlyDictionary<long, List<FamilyLogDto>> logs)
        {
            try
            {
                foreach (KeyValuePair<long, List<FamilyLogDto>> familyLogs in logs)
                {
                    IFamily family = _familyCache.Get(familyLogs.Key);
                    if (family == null)
                    {
                        continue;
                    }

                    _lock.EnterWriteLock();
                    try
                    {
                        foreach (FamilyLogDto log in familyLogs.Value)
                        {
                            family.Logs.Add(log);
                        }

                        family.Logs.Sort((x, y) => DateTime.Compare(y.Timestamp, x.Timestamp)); // Newest -> Older

                        if (family.Logs.Count <= 200)
                        {
                            continue;
                        }

                        for (int i = 200; i < family.Logs.Count; i++)
                        {
                            family.Logs.RemoveAt(i);
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_MANAGER] AddToFamilyLogs", e);
            }
        }

        public void SendLogToFamilyServer(FamilyLogDto log)
        {
            _familyLogManager.SaveLogToBuffer(log);
        }

        public IEnumerable<long> AddToFamilyExperiences(Dictionary<long, long> exps)
        {
            var familyIds = new HashSet<long>();

            foreach ((long characterId, long experience) in exps)
            {
                FamilyMembership membership = _membershipCache.Get(characterId);
                if (membership == null)
                {
                    continue;
                }

                membership.Experience = experience;
                familyIds.Add(membership.FamilyId);
            }

            return familyIds;
        }

        public void SendExperienceToFamilyServer(ExperienceGainedSubMessage experienceGainedSubMessage)
        {
            _familyExperienceManager.SaveFamilyExperienceToBuffer(experienceGainedSubMessage);
        }

        public async Task<bool> CanJoinNewFamilyAsync(int playerEntityId) =>
            await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:character:{playerEntityId}:join-family", DateTime.UtcNow.Date.AddMinutes(5));

        public async Task<bool> RemovePlayerJoinCooldownAsync(int playerEntityId) => await _expirableLockService.TryRemoveTemporaryLock($"game:locks:character:{playerEntityId}:join-family");

        public IReadOnlyCollection<FamilyBuffCrossChannel> CurrentFamilyBuffs => _familyBuffs;
        public void AddFamilyBuff(FamilyBuffCrossChannel familyBuffCrossChannel)
        {
            _familyBuffs.Add(familyBuffCrossChannel);
        }

        public void RemoveFamilyBuff(FamilyBuffCrossChannel familyBuffCrossChannel)
        {
            _familyBuffs.Remove(familyBuffCrossChannel);
        }

        public async Task<bool> AddFamilyBuffAsync(int vnum, long familyId) => 
            await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:family:{familyId}:{vnum}:family-buff", DateTime.UtcNow.Date.AddDays(1));

        public async Task<bool> CanUseFamilyBuffAsync(int vnum, long familyId) => 
            await _expirableLockService.ExistsTemporaryLock($"game:locks:family:{familyId}:{vnum}:family-buff") == false;
        
        public Family GetFamilyByFamilyId(long familyId)
        {
            Family family = _familyCache.Get(familyId);
            if (family != null)
            {
                return family;
            }

            FamilyIdResponse response = _familyService.GetFamilyByIdAsync(new FamilyIdRequest { FamilyId = familyId }).ConfigureAwait(false).GetAwaiter().GetResult();
            family = _familyCache.GetOrSet(familyId, () => new Family(response.Family, response.Members, response.Logs, _sessionManager));
            _familyByName.Set(family.Name, family);
            foreach (FamilyMembership member in family.Members)
            {
                _membershipCache.Set(member.CharacterId, member);
            }

            return family;
        }

        public Family GetFamilyByFamilyIdCache(long familyId) => _familyCache.Get(familyId);
    }
}