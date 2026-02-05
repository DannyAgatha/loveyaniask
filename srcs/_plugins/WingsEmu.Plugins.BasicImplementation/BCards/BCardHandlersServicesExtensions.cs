using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;
using WingsEmu.Game.Buffs;

namespace NosEmu.Plugins.BasicImplementations.BCards;

internal static class BCardHandlersServicesExtensions
{
    internal static void AddBcardHandlers(this IServiceCollection services)
    {
        Type[] tmp = typeof(BCardGamePlugin).Assembly.GetTypesImplementingInterface<IBCardEffectAsyncHandler>();
        foreach (Type handlerType in tmp)
        {
            if (handlerType.IsAbstract)
            {
                continue;
            }

            services.AddTransient(handlerType);
        }

        services.AddSingleton<IBCardEffectHandlerContainer, BCardHandlerContainer>();
    }

    internal static void AddBCardContextFactory(this IServiceCollection services)
    {
        services.AddTransient<IBCardEventContextFactory, BCardEffectContextFactory>();
    }
}