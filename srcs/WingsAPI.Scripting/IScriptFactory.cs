using System.Reflection;

namespace WingsAPI.Scripting
{
    /// <summary>
    ///     Script factory used to instantiate script of defined type
    /// </summary>
    /// <typeparam name="T">Type of the script created by this factory</typeparam>
    public interface IScriptFactory
    {
        void RegisterAllScriptingObjectsInAssembly(Assembly assembly);
        void RegisterType<T>();

        /// <summary>
        ///     Create a new script from file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Script created</returns>
        T LoadScript<T>(string path);
    }
}