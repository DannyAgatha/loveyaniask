using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;
using WingsEmu.Game.Quests;

namespace Plugin.QuestImpl
{
    public static class QuestDependencyInjectionExtensions
    {
        public static void AddRunScriptHandlers(this IServiceCollection services)
        {
            Type[] types = typeof(QuestPlugin).Assembly.GetTypesImplementingInterface<IRunScriptHandler>();
            foreach (Type handlerType in types)
            {
                services.AddTransient(handlerType);
            }

            services.AddSingleton<IRunScriptHandlerContainer, BaseRunScriptHandler>();
        }
    }
}