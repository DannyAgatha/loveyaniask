namespace WingsAPI.Plugins
{
    public class PluginPathConfigurationProvider : IPluginPathConfigurationProvider
    {
        public PluginPathConfigurationProvider(string pluginsPath) => PluginsPath = pluginsPath;

        public string PluginsPath { get; }
    }
}