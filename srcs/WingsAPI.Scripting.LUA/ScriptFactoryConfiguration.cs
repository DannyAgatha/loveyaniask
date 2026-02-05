using System.IO;

namespace WingsAPI.Scripting.LUA
{
    public class ScriptFactoryConfiguration
    {
        public string LibDirectory { get; set; }
        public string RootDirectory { get; set; }
        public string RaidsDirectory => Path.Combine(RootDirectory, "raids");
        public string DungeonsDirectory => Path.Combine(RootDirectory, "dungeons");
        public string TimeSpacesDirectory => Path.Combine(RootDirectory, "timespaces");
        public string MissionsSystemDirectory => Path.Combine(RootDirectory, "mission-system");
    }
}