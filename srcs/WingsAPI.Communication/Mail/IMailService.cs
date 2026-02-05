using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Mail
{
    [ServiceContract]
    public interface IMailService
    {
        [OperationContract]
        ValueTask<CreateMailResponse> CreateMailAsync(CreateMailRequest request);

        [OperationContract]
        Task<CreateMailBatchResponse> CreateMailBatchAsync(CreateMailBatchRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> RemoveMailAsync(RemoveMailRequest request);

        [OperationContract]
        ValueTask<GetMailsResponse> GetMailsByCharacterId(GetMailsRequest request);
    }
}