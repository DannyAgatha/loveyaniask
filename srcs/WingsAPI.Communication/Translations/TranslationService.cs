// NosEmu
// 


using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf;
using WingsAPI.Data.GameData;

namespace WingsAPI.Communication.Translations
{
    [ServiceContract]
    public interface ITranslationService
    {
        [OperationContract]
        Task<GetTranslationsResponse> GetTranslations(EmptyRpcRequest rpcRequest);

        [OperationContract]
        Task<GetForbiddenWordsResponse> GetForbiddenWords(EmptyRpcRequest rpcRequest);
    }

    [ProtoContract]
    public class GetTranslationsResponse
    {
        [ProtoMember(1)]
        public IReadOnlyList<GenericTranslationDto> Translations { get; set; }
    }


    [ProtoContract]
    public class GetForbiddenWordsResponse
    {
        [ProtoMember(1)]
        public IReadOnlyList<string> ForbiddenWords { get; set; }
    }
}