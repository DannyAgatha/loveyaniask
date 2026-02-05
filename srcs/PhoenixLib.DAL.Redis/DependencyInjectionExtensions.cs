using Foundatio.Caching;
using Foundatio.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace PhoenixLib.DAL.Redis
{
    public static class DependencyInjectionExtensions
    {
        public static IConnectionMultiplexer GetConnectionMultiplexer(this RedisConfiguration redisConfiguration) =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                Password = redisConfiguration.Password,
                EndPoints = { $"{redisConfiguration.Host}:{redisConfiguration.Port}" }
            });

        /// <summary>
        ///     Register redis configuration from env
        /// </summary>
        /// <param name="services"></param>
        public static void TryAddConfigurationFromEnv(this IServiceCollection services)
        {
            services.TryAddSingleton(s => RedisConfiguration.FromEnv());
        }

        /// <summary>
        ///     Registers the Connection Multiplexer
        /// </summary>
        /// <param name="services"></param>
        internal static void TryAddConnectionMultiplexer(this IServiceCollection services)
        {
            services.TryAddSingleton(s => s.GetService<RedisConfiguration>().GetConnectionMultiplexer());
        }

        /// <summary>
        ///     Registers the Connection Multiplexer
        /// </summary>
        /// <param name="services"></param>
        public static void TryAddConnectionMultiplexerFromEnv(this IServiceCollection services)
        {
            services.TryAddConfigurationFromEnv();
            services.TryAddConnectionMultiplexer();
        }

        public static void TryAddRedisCacheClient(this IServiceCollection services)
        {
            services.TryAddSingleton(s => new RedisCacheClient(new RedisCacheClientOptions
            {
                ConnectionMultiplexer = s.GetRequiredService<IConnectionMultiplexer>(),
                Serializer = new JsonNetSerializer()
            }));
            services.TryAddSingleton<ICacheClient>(s => s.GetRequiredService<RedisCacheClient>());
        }

        /// <summary>
        ///     Registers a KeyValueStorage from Env
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        public static void TryAddRedisKeyValueStorageFromEnv<TObject, TKey>(this IServiceCollection services)
        {
            services.TryAddConnectionMultiplexerFromEnv();
            services.TryAddRedisCacheClient();
            services.TryAddSingleton(typeof(IKeyValueAsyncStorage<,>), typeof(RedisGenericKeyValueAsyncStorage<,>));
        }
    }
}