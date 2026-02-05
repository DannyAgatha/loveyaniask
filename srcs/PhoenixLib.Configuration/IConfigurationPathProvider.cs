namespace PhoenixLib.Configuration
{
    public interface IConfigurationPathProvider
    {
        string GetConfigurationPath(string configBlobName);
    }
}