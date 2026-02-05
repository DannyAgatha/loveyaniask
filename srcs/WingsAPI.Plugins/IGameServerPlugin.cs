using Microsoft.Extensions.DependencyInjection;

namespace WingsAPI.Plugins
{
    public interface IGameServerPlugin : IPlugin
    {
        void AddDependencies(IServiceCollection services, GameServerLoader gameServer);
    }
}