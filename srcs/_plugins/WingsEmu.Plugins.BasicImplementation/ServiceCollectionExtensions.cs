using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;
using WingsEmu.Game._Guri;
using WingsEmu.Game.Core.SkillHandling;

namespace NosEmu.Plugins.BasicImplementations;

public static class ServiceCollectionExtensions
{
    public static void AddSkillHandlers(this IServiceCollection services)
    {
        Type[] types = typeof(GuriPlugin).Assembly.GetTypesImplementingInterface<ISkillHandler>();
        foreach (Type handlerType in types)
        {
            services.AddTransient(handlerType);
        }

        services.AddSingleton<ISkillHandlerContainer, BaseSkillHandler>();
    }
    public static void AddGuriHandlers(this IServiceCollection services)
    {
        Type[] types = typeof(GuriPlugin).Assembly.GetTypesImplementingInterface<IGuriHandler>();
        foreach (Type handlerType in types)
        {
            services.AddTransient(handlerType);
        }

        services.AddSingleton<IGuriHandlerContainer, BaseGuriHandler>();
    }
}