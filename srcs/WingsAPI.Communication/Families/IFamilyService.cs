// NosEmu
// 


using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Families
{
    [ServiceContract]
    public interface IFamilyService
    {
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [OperationContract]
        ValueTask<FamilyCreateResponse> CreateFamilyAsync(FamilyCreateRequest name);

        [OperationContract]
        ValueTask<BasicRpcResponse> DisbandFamilyAsync(FamilyDisbandRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> ChangeAuthorityByIdAsync(FamilyChangeAuthorityRequest request);

        [OperationContract]
        ValueTask<FamilyChangeFactionResponse> ChangeFactionByIdAsync(FamilyChangeFactionRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> ChangeTitleByIdAsync(FamilyChangeTitleRequest request);

        [OperationContract]
        ValueTask<FamilyUpgradeResponse> TryAddFamilyUpgrade(FamilyUpgradeRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> AddMemberToFamilyAsync(FamilyAddMemberRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> MemberDisconnectedAsync(FamilyMemberDisconnectedRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> RemoveMemberToFamilyAsync(FamilyRemoveMemberRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> RemoveMemberByCharIdAsync(FamilyRemoveMemberByCharIdRequest request);

        [OperationContract]
        ValueTask<FamilyIdResponse> GetFamilyByIdAsync(FamilyIdRequest req);

        [OperationContract]
        ValueTask<FamilyListMembersResponse> GetFamilyMembersByFamilyId(FamilyIdRequest req);

        [OperationContract]
        ValueTask<MembershipResponse> GetMembershipByCharacterIdAsync(MembershipRequest req);

        [OperationContract]
        ValueTask<MembershipTodayResponse> CanPerformTodayMessageAsync(MembershipTodayRequest req);

        [OperationContract]
        ValueTask<BasicRpcResponse> UpdateFamilySettingsAsync(FamilySettingsRequest request);
        
        [OperationContract]
        ValueTask<FamilyReputationRankingResponse> GetFamilyReputation(FamilyReputationRankingRequest req);
        
        [OperationContract]
        ValueTask<FamilyAllResponse> GetFamilies();
        
        [OperationContract]
        ValueTask<EmptyResponse> SaveFamilyAsync(FamilySaveRequest req);

        [OperationContract]
        ValueTask<EmptyResponse> CreateFamilyBuffAsync(FamilyBuffCreateRequest req);

        [OperationContract]
        ValueTask<EmptyResponse> ResetFamilyMissions();
    }
}