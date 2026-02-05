using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Mail
{
    [ServiceContract]
    public interface INoteService
    {
        [OperationContract]
        ValueTask<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> OpenNoteAsync(OpenNoteRequest request);

        [OperationContract]
        ValueTask<BasicRpcResponse> RemoveNoteAsync(RemoveNoteRequest request);
    }
}