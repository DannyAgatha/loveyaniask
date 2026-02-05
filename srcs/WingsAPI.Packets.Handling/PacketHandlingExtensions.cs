using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;

namespace WingsAPI.Packets.Handling
{
    public static class PacketHandlingExtensions
    {
        public static void AddGamePacketHandlersInAssembly<T>(this IServiceCollection services)
        {
            services.AddGamePacketHandlersInAssembly(typeof(T).Assembly);
        }

        public static void AddGamePacketHandlersInAssembly(this IServiceCollection services, Assembly assembly)
        {
            Type[] types = assembly.GetTypesImplementingGenericClass(typeof(GenericGamePacketHandlerBase<>));
            services.AddGamePacketHandlersInAssembly(types);
        }

        public static void AddGamePacketHandlersInAssembly(this IServiceCollection services, Type[] types)
        {
            foreach (Type type in types)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                services.AddSingleton(new RegisteredPacketHandler { HandlerType = type });
                services.AddTransient(type);
            }
        }
    }
}