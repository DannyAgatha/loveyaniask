namespace PhoenixLib.Configuration
{
    public class ConfigurationPathProvider : IConfigurationPathProvider
    {
        private readonly string _path;

        public ConfigurationPathProvider(string path) => _path = path;

        public string GetConfigurationPath(string configBlobName) => _path + configBlobName;
    }
}