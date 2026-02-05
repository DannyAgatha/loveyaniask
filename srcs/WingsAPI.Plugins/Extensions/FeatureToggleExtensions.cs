// NosEmu
// 


using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Extensions;

namespace WingsAPI.Plugins.Extensions
{
    public static class AssemblyExtensions
    {
        public static void AddTypesImplementingInterfaceInAssembly<T>(this IServiceCollection services, Assembly assembly)
        {
            Type[] types = assembly.GetTypesImplementingInterface<T>();
            foreach (Type handlerType in types)
            {
                services.AddTransient(handlerType);
            }
        }
    }

    public static class FeatureToggleExtensions
    {
        public static void TryAddSingletonFeatureToggleEnabledByDefault<TInterface, TImplementation>(this IServiceCollection services, string envVarName)
        where TInterface : class
        where TImplementation : class, TInterface
        {
            services.TryAddSingletonFeatureToggle<TInterface, TImplementation>(envVarName, true);
        }

        public static void TryAddSingletonFeatureToggleDisabledByDefault<TInterface, TImplementation>(this IServiceCollection services, string envVarName)
        where TInterface : class
        where TImplementation : class, TInterface
        {
            services.TryAddSingletonFeatureToggle<TInterface, TImplementation>(envVarName, false);
        }

        public static void TryAddSingletonFeatureToggle<TInterface, TImplementation>(this IServiceCollection services, string envVarName, bool defaultActivationState)
        where TInterface : class
        where TImplementation : class, TInterface

        {
            if (!bool.TryParse(Environment.GetEnvironmentVariable(envVarName) ?? defaultActivationState.ToString(), out bool isActivated))
            {
                return;
            }

            if (!isActivated)
            {
                return;
            }

            services.TryAddSingleton<TInterface, TImplementation>();
        }

        public static void TryAddTransientFeatureToggleEnabledByDefault<TInterface, TImplementation>(this IServiceCollection services, string envVarName)
        where TInterface : class
        where TImplementation : class, TInterface
        {
            services.TryAddTransientFeatureToggle<TInterface, TImplementation>(envVarName, true);
        }

        public static void TryAddTransientFeatureToggleDisabledByDefault<TInterface, TImplementation>(this IServiceCollection services, string envVarName)
        where TInterface : class
        where TImplementation : class, TInterface
        {
            services.TryAddTransientFeatureToggle<TInterface, TImplementation>(envVarName, false);
        }

        public static void TryAddTransientFeatureToggle<TInterface, TImplementation>(this IServiceCollection services, string envVarName, bool defaultActivationState)
        where TInterface : class
        where TImplementation : class, TInterface

        {
            if (!bool.TryParse(Environment.GetEnvironmentVariable(envVarName) ?? defaultActivationState.ToString(), out bool isActivated))
            {
                return;
            }

            if (!isActivated)
            {
                return;
            }

            services.TryAddTransient<TInterface, TImplementation>();
        }
    }
}