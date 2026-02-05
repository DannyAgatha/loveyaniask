using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PhoenixLib.Auth.JWT
{
    public static class DependencyInjectionExtensions
    {
        public static void AddJwtFactoryFromEnv(this IServiceCollection services)
        {
            services.TryAddSingleton<IJwtTokenFactory>(new JwtTokenFactory(Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY")));
        }
    }
}