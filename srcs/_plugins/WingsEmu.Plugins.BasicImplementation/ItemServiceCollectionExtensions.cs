using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;

namespace NosEmu.Plugins.BasicImplementations;

public static class ItemServiceCollectionExtensions
{
    public static void AddHandlers<TPlugin, T>(this IServiceCollection services)
    {
        Type[] types = typeof(TPlugin).Assembly.GetTypesImplementingInterface<T>();
        foreach (Type handlerType in types)
        {
            services.AddTransient(handlerType);
        }
    }
}