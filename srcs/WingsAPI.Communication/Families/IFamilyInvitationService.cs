using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Families
{
    [ServiceContract]
    public interface IFamilyInvitationService
    {
        [OperationContract]
        ValueTask<EmptyResponse> SaveFamilyInvitationAsync(FamilyInvitationSaveRequest request);

        [OperationContract]
        ValueTask<FamilyInvitationContainsResponse> ContainsFamilyInvitationAsync(FamilyInvitationRequest request);

        [OperationContract]
        ValueTask<FamilyInvitationGetResponse> GetFamilyInvitationAsync(FamilyInvitationRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> RemoveFamilyInvitationAsync(FamilyInvitationRemoveRequest request);
    }
}