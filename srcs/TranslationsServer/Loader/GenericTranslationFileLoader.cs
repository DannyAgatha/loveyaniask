using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Data.GameData;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TranslationServer.Loader
{
    public class GenericTranslationFileLoader : IResourceLoader<GenericTranslationDto>
    {
        private static readonly INamingConvention __NamingConvention = UnderscoredNamingConvention.Instance;
        private static readonly IDeserializer __Deserializer = new DeserializerBuilder().WithNamingConvention(__NamingConvention).Build();


        private readonly TranslationsFileLoaderOptions _options;
        private readonly List<GenericTranslationDto> _translations = new();

        public GenericTranslationFileLoader(TranslationsFileLoaderOptions options) => _options = options;

        public async Task<IReadOnlyList<GenericTranslationDto>> LoadAsync()
        {
            if (_translations.Any())
            {
                return _translations;
            }

            Dictionary<string, string> english = null;
            foreach (RegionLanguageType i in Enum.GetValues(typeof(RegionLanguageType)))
            {
                if (i == RegionLanguageType.RU)
                {
                    continue;
                }

                string languageType = i.ToString().ToLowerInvariant();
                var newTmp = new Dictionary<string, string>();

                string languageDirectory = Path.Combine(_options.TranslationsPath, $"{languageType}");
                foreach (string translationFile in Directory.GetFiles(languageDirectory, "*.yml").Concat(Directory.GetFiles(languageDirectory, "*.yaml")))
                {
                    try
                    {
                        string fileContent = await File.ReadAllTextAsync(translationFile);
                        IDeserializer deserializer = __Deserializer;
                        Dictionary<string, string> tmp = deserializer.Deserialize<Dictionary<string, string>>(fileContent);

                        foreach ((string s, string value) in tmp)
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                continue;
                            }

                            if (value == $"#{s}" && english != null && english.TryGetValue(s, out string translated))
                            {
                                newTmp[s] = translated;
                                continue;
                            }

                            newTmp[s] = value;
                        }

                        if (i == RegionLanguageType.EN)
                        {
                            english = newTmp;
                        }

                        _translations.AddRange(newTmp.Select(s => new GenericTranslationDto
                        {
                            Key = s.Key,
                            Value = s.Value,
                            Language = i
                        }));
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[RESOURCE_LOADER] {translationFile} {languageType}", e);
                    }
                }
            }

            Log.Info($"[RESOURCE_LOADER] {_translations.Count.ToString()} translations loaded");
            return _translations;
        }
    }
}