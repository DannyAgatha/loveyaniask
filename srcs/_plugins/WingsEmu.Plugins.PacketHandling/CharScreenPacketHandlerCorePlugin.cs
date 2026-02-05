// NosEmu
// 


using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;
using WingsAPI.Plugins;

namespace WingsEmu.Plugins.PacketHandling;

public class CharScreenPacketHandlerCorePlugin : IGameServerPlugin
{
    public string Name => nameof(CharScreenPacketHandlerCorePlugin);


    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        Type[] handlerTypes = typeof(CharScreenPacketHandlerCorePlugin).Assembly.GetTypesImplementingGenericClass(typeof(GenericCharScreenPacketHandlerBase<>));

        foreach (Type handlerType in handlerTypes)
        {
            services.AddTransient(handlerType);
        }
    }
}