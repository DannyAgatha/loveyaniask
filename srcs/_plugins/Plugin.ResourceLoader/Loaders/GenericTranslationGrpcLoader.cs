using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Translations;
using WingsAPI.Data.GameData;

namespace Plugin.ResourceLoader.Loaders
{
    public class GenericTranslationGrpcLoader : IResourceLoader<GenericTranslationDto>
    {
        private readonly ITranslationService _config;

        public GenericTranslationGrpcLoader(ITranslationService config) => _config = config;

        public async Task<IReadOnlyList<GenericTranslationDto>> LoadAsync() => (await _config.GetTranslations(new EmptyRpcRequest())).Translations;
    }
}