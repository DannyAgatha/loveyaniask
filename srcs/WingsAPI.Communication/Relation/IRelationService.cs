using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Relation
{
    [ServiceContract]
    public interface IRelationService
    {
        [OperationContract]
        Task<RelationAddResponse> AddRelationAsync(RelationAddRequest request);

        [OperationContract]
        Task<RelationGetAllResponse> GetRelationsByIdAsync(RelationGetAllRequest request);

        [OperationContract]
        Task<BasicRpcResponse> RemoveRelationAsync(RelationRemoveRequest request);
    }
}