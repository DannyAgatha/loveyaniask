using System.IO;

namespace Plugin.ResourceLoader
{
    public class ResourceLoadingConfiguration
    {
        public ResourceLoadingConfiguration(string resourcesPath) => ResourcePaths = resourcesPath;

        public string ResourcePaths { get; }
        public string GameDataPath => Path.Combine(ResourcePaths, "dat");
        public string GameMapsPath => Path.Combine(ResourcePaths, "maps");
        public string GameLanguagePath => Path.Combine(ResourcePaths, "lang");
        public string GenericTranslationsPath => Path.Combine(ResourcePaths, "translations");
    }
}