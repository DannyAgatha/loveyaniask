// NosEmu
// 


using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.DAL.Redis;

namespace PhoenixLib.MultiLanguage
{
    public static class DependencyInjectionExtensions
    {
        public static void TryAddEnumBasedMultilanguageService<T>(this IServiceCollection services)
        where T : struct, Enum
        {
            services.TryAddConnectionMultiplexerFromEnv();
            services.AddSingleton<IEnumBasedLanguageService<T>, GenericMultilanguageService<T>>();
        }
    }
}