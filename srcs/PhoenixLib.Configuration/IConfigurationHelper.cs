namespace PhoenixLib.Configuration
{
    /// <summary>
    ///     A file system based configuration helper
    /// </summary>
    public interface IConfigurationHelper
    {
        T Load<T>(string path) where T : class, new();
        T Load<T>(string path, bool createIfNotExists) where T : class, new();
        T Load<T>(string path, T defaultValue) where T : class, new();
        void Save<T>(string path, T value);
    }
}