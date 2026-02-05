// NosEmu
// 


using Microsoft.Extensions.DependencyInjection;

namespace WingsAPI.Plugins
{
    /// <summary>
    ///     Plugins that injects dependencies
    /// </summary>
    public interface IDependencyInjectorPlugin : IPlugin
    {
        /// <summary>
        ///     Loads the plugin with the given container builder to register dependencies
        /// </summary>
        /// <param name="services"></param>
        void AddDependencies(IServiceCollection services);
    }
}