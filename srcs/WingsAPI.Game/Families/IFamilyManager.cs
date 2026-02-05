using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Families;

namespace WingsEmu.Game.Families;

public interface IFamilyManager
{
    void AddFamily(FamilyDTO family, IReadOnlyCollection<FamilyMembershipDto> members);
    void RemoveFamily(long familyId);
    IFamily GetFamilyByFamilyName(string familyName);
    Family GetFamilyByFamilyId(long familyId);
    Family GetFamilyByFamilyIdCache(long familyId);
    void AddOrReplaceMember(FamilyMembershipDto membership);
    void AddOrReplaceMember(FamilyMembershipDto membership, IFamily family);
    void RemoveMember(long characterId, long familyId);
    void MemberDisconnectionUpdate(long characterId, DateTime disconnectionTime);
    FamilyMembership GetFamilyMembershipByCharacterId(long characterId);
    void AddToFamilyLogs(IReadOnlyDictionary<long, List<FamilyLogDto>> logs);
    void SendLogToFamilyServer(FamilyLogDto log);
    IEnumerable<long> AddToFamilyExperiences(Dictionary<long, long> exps);
    void SendExperienceToFamilyServer(ExperienceGainedSubMessage experienceGainedSubMessage);
    Task<bool> CanJoinNewFamilyAsync(int playerEntityId);
    Task<bool> RemovePlayerJoinCooldownAsync(int playerEntityId);
    
    IReadOnlyCollection<FamilyBuffCrossChannel> CurrentFamilyBuffs { get; }
    void AddFamilyBuff(FamilyBuffCrossChannel familyBuffCrossChannel);
    void RemoveFamilyBuff(FamilyBuffCrossChannel familyBuffCrossChannel);
    Task<bool> AddFamilyBuffAsync(int vnum, long familyId);
    Task<bool> CanUseFamilyBuffAsync(int vnum, long familyId);
}