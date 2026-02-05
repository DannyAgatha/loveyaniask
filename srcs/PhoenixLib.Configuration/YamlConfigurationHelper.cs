using System.Diagnostics.CodeAnalysis;
using System.IO;
using PhoenixLib.Logging;
using YamlDotNet.Serialization;

namespace PhoenixLib.Configuration
{
    /// <summary>
    ///     Yaml
    /// </summary>
    public class YamlConfigurationHelper : IConfigurationHelper
    {
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;

        public YamlConfigurationHelper(IDeserializer deserializer, ISerializer serializer)
        {
            _deserializer = deserializer;
            _serializer = serializer;
        }

        public T Load<T>([NotNull] string path) where T : class, new() => Load<T>(path, false);

        public T Load<T>([NotNull] string path, bool createIfNotExists) where T : class, new()
        {
            if (createIfNotExists)
            {
                return Load(path, new T());
            }

            if (!File.Exists(path))
            {
                throw new IOException(path);
            }

            Log.Debug($"[CONFIGURATION_HELPER] Loading configuration {typeof(T).Name} from {path}...");

            using FileStream stream = File.OpenRead(path);

            Log.Debug($"[CONFIGURATION_HELPER] Loading configuration {typeof(T).Name} from {path}...");

            using var streamReader = new StreamReader(stream);

            T deserialized = _deserializer.Deserialize<T>(streamReader);
            Log.Debug($"[CONFIGURATION_HELPER] Configuration {typeof(T).Name} from {path} loaded !");

            return deserialized;
        }

        public T Load<T>([NotNull] string path, T defaultValue) where T : class, new()
        {
            if (!File.Exists(path))
            {
                Log.Debug($"[CONFIGURATION_HELPER] Configuration at {path}, does not exist, creating a new one...");
                Save(path, defaultValue);
            }

            using FileStream stream = File.OpenRead(path);

            Log.Debug($"[CONFIGURATION_HELPER] Loading configuration {typeof(T).Name} from {path}...");

            using var streamReader = new StreamReader(stream);

            T deserialized = _deserializer.Deserialize<T>(streamReader);

            Log.Debug($"[CONFIGURATION_HELPER] Configuration {typeof(T).Name} from {path} loaded !");
            return deserialized;
        }

        public void Save<T>([NotNull] string path, T value)
        {
            if (!Directory.Exists(path))
            {
                Log.Debug($"[CONFIGURATION_HELPER] Creating directory {path}...");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            Log.Debug($"[CONFIGURATION_HELPER] Saving configuration {typeof(T).Name} into {path}...");
            string valueSerialized = _serializer.Serialize(value);
            File.WriteAllText(path, valueSerialized);
            Log.Debug($"[CONFIGURATION_HELPER] Configuration saved into {path} !");
        }
    }
}