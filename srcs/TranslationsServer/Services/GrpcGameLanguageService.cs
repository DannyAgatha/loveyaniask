// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using TranslationServer.Loader;
using WingsAPI.Communication;
using WingsAPI.Communication.Translations;
using WingsAPI.Data.GameData;

namespace TranslationServer.Services
{
    public class GrpcGameLanguageService : ITranslationService
    {
        private readonly BannedNamesConfiguration _bannedNamesConfiguration;
        private readonly IResourceLoader<GenericTranslationDto> _loader;

        public GrpcGameLanguageService(IResourceLoader<GenericTranslationDto> loader, BannedNamesConfiguration bannedNamesConfiguration)
        {
            _loader = loader;
            _bannedNamesConfiguration = bannedNamesConfiguration;
        }

        public async Task<GetTranslationsResponse> GetTranslations(EmptyRpcRequest rpcRequest)
        {
            IReadOnlyList<GenericTranslationDto> tmp = await _loader.LoadAsync();
            return new GetTranslationsResponse
            {
                Translations = tmp
            };
        }

        public async Task<GetForbiddenWordsResponse> GetForbiddenWords(EmptyRpcRequest rpcRequest) =>
            new GetForbiddenWordsResponse
            {
                ForbiddenWords = _bannedNamesConfiguration.BannedNames
            };
    }
}