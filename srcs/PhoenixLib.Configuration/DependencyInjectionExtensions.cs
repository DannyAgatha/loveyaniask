using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Logging;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PhoenixLib.Configuration
{
    public class YamlNullableEnumTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => Nullable.GetUnderlyingType(type)?.IsEnum ?? false;

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            type = Nullable.GetUnderlyingType(type) ?? throw new ArgumentException("Expected nullable enum type for ReadYaml");
            Scalar scalar = parser.Consume<Scalar>();

            if (string.IsNullOrWhiteSpace(scalar.Value))
            {
                return null;
            }

            try
            {
                return Enum.Parse(type, scalar.Value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid value: \"{scalar.Value}\" for {type.Name}", ex);
            }
        }

        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            type = Nullable.GetUnderlyingType(type) ?? throw new ArgumentException("Expected nullable enum type for WriteYaml");

            if (value == null)
            {
                return;
            }

            string? toWrite = Enum.GetName(type, value) ?? throw new InvalidOperationException($"Invalid value {value} for enum: {type}");
            emitter.Emit(new Scalar(null, null, toWrite, ScalarStyle.Any, true, false));
        }
    }

    public static class DependencyInjectionExtensions
    {
        public static void AddYamlConfigurationHelper(this IServiceCollection services,
            Action<ConfigurationHelperConfig> configurationAction = null)
        {
            var config = new ConfigurationHelperConfig();

            configurationAction?.Invoke(config);

            services.AddSingleton(UnderscoredNamingConvention.Instance);
            services.AddSingleton(s =>
                new SerializerBuilder().WithNamingConvention(s.GetRequiredService<INamingConvention>()).WithTypeConverter(new YamlNullableEnumTypeConverter()).Build());
            services.AddSingleton(s =>
                new DeserializerBuilder().WithNamingConvention(s.GetRequiredService<INamingConvention>()).WithTypeConverter(new YamlNullableEnumTypeConverter()).Build());
            services.AddTransient<IConfigurationHelper, YamlConfigurationHelper>();
            services.AddSingleton<IConfigurationPathProvider>(new ConfigurationPathProvider(config.ConfigurationDirectory));
        }

        /// <summary>
        ///     The file will be named with underscore case with .yaml as its file extension
        /// </summary>
        /// <param name="services"></param>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddConfigurationsFromDirectory<T>(this IServiceCollection services, string path)
        where T : class, new()
        {
            services.AddSingleton<IEnumerable<T>>(s =>
                s.GetRequiredService<IConfigurationHelper>()
                    .GetConfigurations<T>(s.GetService<IConfigurationPathProvider>().GetConfigurationPath(path)));
        }

        /// <summary>
        ///     The file will be named with underscore case with .yaml as its file extension
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configName"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddFileConfiguration<T>(this IServiceCollection services, string configName)
        where T : class, new()
        {
            services.AddSingleton(s =>
                s.GetRequiredService<IConfigurationHelper>().Load<T>(
                    s.GetService<IConfigurationPathProvider>().GetConfigurationPath(configName.ToUnderscoreCase() + ".yaml"), true));
        }

        /// <summary>
        ///     The file will be named with underscore case with .yaml as its file extension
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configName"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddMultipleConfigurationOneFile<T>(this IServiceCollection services, string configName)
        where T : class, new()
        {
            services.AddSingleton<IEnumerable<T>>(s =>
                s.GetRequiredService<IConfigurationHelper>().Load<List<T>>(
                    s.GetService<IConfigurationPathProvider>().GetConfigurationPath(configName.ToUnderscoreCase() + ".yaml"), true));
        }


        /// <summary>
        ///     The file will be named with underscore case with .yaml as its file extension
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddFileConfiguration<T>(this IServiceCollection services) where T : class, new()
        {
            services.AddSingleton(s =>
                s.GetRequiredService<IConfigurationHelper>().Load<T>(
                    s.GetService<IConfigurationPathProvider>().GetConfigurationPath(typeof(T).Name.ToUnderscoreCase() + ".yaml"),
                    true));
        }


        /// <summary>
        ///     The file will be named with underscore case with .yaml as its file extension
        /// </summary>
        /// <param name="services"></param>
        /// <param name="defaultConfig">Default value in case the file does not exist</param>
        /// <typeparam name="T"></typeparam>
        public static void AddFileConfiguration<T>(this IServiceCollection services, T defaultConfig)
        where T : class, new()
        {
            services.AddSingleton(s =>
                s.GetRequiredService<IConfigurationHelper>().Load(
                    s.GetService<IConfigurationPathProvider>().GetConfigurationPath(typeof(T).Name.ToUnderscoreCase() + ".yaml"),
                    defaultConfig));
        }

        private static List<T> GetConfigurations<T>(this IConfigurationHelper helper, string path)
        where T : class, new()
        {
            var configs = new List<T>();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return configs;
            }

            foreach (string file in Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories).Concat(Directory.GetFiles(path, "*.yml", SearchOption.AllDirectories)))
            {
                var fileInfo = new FileInfo(file);
                T config = helper.Load<T>(file);
                configs.Add(config);

                Log.Info($"[CONFIGURATION_HELPER] Loading configuration from {fileInfo.Name} as {typeof(T).Name}");
            }

            return configs;
        }
    }
}