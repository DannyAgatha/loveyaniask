using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WingsEmu.Packets;

namespace WingsAPI.Packets
{
    public static class PacketExtensions
    {
        public static void AddClientPacketsInAssembly<T>(this IServiceCollection services)
        {
            IEnumerable<Type> types = typeof(T).Assembly.GetTypes().Where(s => !s.IsInterface && !s.IsAbstract && typeof(ClientPacket).IsAssignableFrom(s) && s != typeof(UnresolvedPacket)).ToArray();

            foreach (Type type in types)
            {
                services.AddSingleton(new ClientPacketRegistered
                {
                    PacketType = type
                });
            }
        }
    }
}