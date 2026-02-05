// NosEmu
// 


using System.IO;

namespace WingsAPI.Plugins
{
    public interface IPluginManager
    {
        IPlugin[] LoadPlugin(FileInfo file);
        IPlugin[] LoadPlugins(DirectoryInfo directory);
    }
}